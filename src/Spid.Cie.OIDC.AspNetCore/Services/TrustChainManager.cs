using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

internal class TrustChainManager : ITrustChainManager
{
    private readonly HttpClient _httpClient;
    private readonly ICryptoService _cryptoService;
    private readonly ILogPersister _logPersister;
    private readonly ILogger<TrustChainManager> _logger;
    private readonly IMetadataPolicyHandler _metadataPolicyHandler;
    private static readonly Dictionary<string, KeyValuePair<DateTimeOffset, IdPEntityConfiguration>> _trustChainCache = new Dictionary<string, KeyValuePair<DateTimeOffset, IdPEntityConfiguration>>();
    private static readonly SemaphoreSlim _syncLock = new SemaphoreSlim(1);

    public TrustChainManager(IHttpClientFactory httpClientFactory,
        ICryptoService cryptoService,
        IMetadataPolicyHandler metadataPolicyHandler,
        ILogPersister logPersister,
        ILogger<TrustChainManager> logger)
    {
        _httpClient = httpClientFactory.CreateClient(SpidCieConst.BackchannelClientName);
        _cryptoService = cryptoService;
        _logPersister = logPersister;
        _logger = logger;
        _metadataPolicyHandler = metadataPolicyHandler;
    }

    public async Task<IdPEntityConfiguration?> BuildTrustChain(string url)
    {
        if (!_trustChainCache.ContainsKey(url) || _trustChainCache[url].Key < DateTimeOffset.UtcNow)
        {
            if (!await _syncLock.WaitAsync(TimeSpan.FromSeconds(10)))
            {
                _logger.LogWarning("TrustChain cache Sync Lock expired.");
                return default;
            }
            if (!_trustChainCache.ContainsKey(url) || _trustChainCache[url].Key < DateTimeOffset.UtcNow)
            {
                try
                {
                    (IdPEntityConfiguration? opConf, string? opDecodedJwt, string? opJwt) = await ValidateAndDecodeEntityConfiguration<IdPEntityConfiguration>(url);
                    if (opConf is null || opJwt is null)
                    {
                        _logger.LogWarning($"EntityConfiguration not retrieved for OP {url}");
                        return default;
                    }

                    DateTimeOffset expiresOn = opConf.ExpiresOn;

                    bool opValidated = false;
                    foreach (var authorityHint in opConf.AuthorityHints)
                    {
                        (TAEntityConfiguration? taConf, string? taDecodedJwt, string? taJwt) = await ValidateAndDecodeEntityConfiguration<TAEntityConfiguration>(authorityHint);
                        if (taConf is null)
                        {
                            _logger.LogWarning($"EntityConfiguration not retrieved for TA {authorityHint}");
                            continue;
                        }
                        if (taConf.ExpiresOn < expiresOn)
                            expiresOn = taConf.ExpiresOn;

                        var fetchUrl = $"{taConf.Metadata.FederationEntity.FederationFetchEndpoint.EnsureTrailingSlash()}?sub={url}";
                        var entityStatement = await GetAndValidateEntityStatement(fetchUrl, opJwt);
                        var esExpiresOn = entityStatement.ExpiresOn;

                        // Apply policy
                        opConf!.Metadata!.OpenIdProvider = _metadataPolicyHandler.ApplyMetadataPolicy(opDecodedJwt!, entityStatement.MetadataPolicy.ToJsonString());

                        if (opConf is not null && opConf.Metadata?.OpenIdProvider is not null)
                        {
                            if (esExpiresOn < expiresOn)
                                expiresOn = esExpiresOn;

                            opValidated = true;
                            break;
                        }
                    }
                    if (opValidated && opConf is not null)
                    {
                        _trustChainCache.Add(url, new KeyValuePair<DateTimeOffset, IdPEntityConfiguration>(expiresOn, opConf));
                    }
                }
                finally
                {
                    _syncLock.Release();
                }
            }
        }
        return _trustChainCache.ContainsKey(url) ? _trustChainCache[url].Value : null;
    }

    private async Task<(T? conf, string? decodedJwt, string? jwt)> ValidateAndDecodeEntityConfiguration<T>(string? url)
        where T : FederationEntityConfiguration
    {
        try
        {
            Throw<Exception>.If(string.IsNullOrWhiteSpace(url), "Url parameter is not defined");

            var metadataAddress = $"{url!.EnsureTrailingSlash()}{SpidCieConst.EntityConfigurationPath}";
            var jwt = await _httpClient.GetStringAsync(metadataAddress);
            Throw<Exception>.If(string.IsNullOrWhiteSpace(jwt), $"EntityConfiguration JWT not retrieved from url {metadataAddress}");

            await _logPersister.LogGetEntityConfiguration(metadataAddress, jwt);

            var decodedJwt = _cryptoService.DecodeJWT(jwt);
            Throw<Exception>.If(string.IsNullOrWhiteSpace(decodedJwt), $"Invalid EntityConfiguration JWT for url {metadataAddress}: {jwt}");

            var conf = JsonSerializer.Deserialize<T>(decodedJwt);
            Throw<Exception>.If(conf is null, $"Invalid Decoded EntityConfiguration JWT for url {metadataAddress}: {decodedJwt}");

            var decodedJwtHeader = _cryptoService.DecodeJWTHeader(jwt);
            Throw<Exception>.If(string.IsNullOrWhiteSpace(decodedJwtHeader), $"Invalid EntityConfiguration JWT Header for url {metadataAddress}: {jwt}");

            var header = JObject.Parse(decodedJwtHeader);
            var kid = (string)header[SpidCieConst.Kid];
            Throw<Exception>.If(string.IsNullOrWhiteSpace(kid), $"No Kid specified in the EntityConfiguration JWT Header for url {metadataAddress}: {decodedJwtHeader}");

            var key = conf!.JWKS.Keys.FirstOrDefault(k => k.kid.Equals(kid, StringComparison.InvariantCultureIgnoreCase));
            Throw<Exception>.If(key is null, $"No key found with kid {kid} for url {metadataAddress}: {decodedJwtHeader}");

            RSA publicKey = _cryptoService.GetRSAPublicKey(key!);
            Throw<Exception>.If(!decodedJwt.Equals(_cryptoService.ValidateJWTSignature(jwt, publicKey)),
                $"Invalid Signature for the EntityConfiguration JWT retrieved at the url {metadataAddress}: {decodedJwtHeader}");

            return (conf, decodedJwt, jwt);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return default;
        }
    }

    private async Task<EntityStatement> GetAndValidateEntityStatement(string? url, string opJwt)
    {
        try
        {
            Throw<Exception>.If(string.IsNullOrWhiteSpace(url), "Url parameter is not defined");

            var esJwt = await _httpClient.GetStringAsync(url);
            Throw<Exception>.If(string.IsNullOrWhiteSpace(esJwt), $"EntityStatement JWT not retrieved from url {url}");

            await _logPersister.LogGetEntityStatement(url!, esJwt);

            var decodedEsJwt = _cryptoService.DecodeJWT(esJwt);
            Throw<Exception>.If(string.IsNullOrWhiteSpace(decodedEsJwt), $"Invalid EntityStatement JWT for url {url}: {esJwt}");

            var entityStatement = JsonSerializer.Deserialize<EntityStatement>(decodedEsJwt);
            Throw<Exception>.If(entityStatement is null, $"Invalid Decoded EntityStatement JWT for url {url}: {decodedEsJwt}");

            var decodedOpJwtHeader = _cryptoService.DecodeJWTHeader(opJwt);
            Throw<Exception>.If(string.IsNullOrWhiteSpace(decodedOpJwtHeader), $"Invalid EntityConfiguration JWT Header: {opJwt}");

            var opHeader = JObject.Parse(decodedOpJwtHeader);
            var kid = (string)opHeader[SpidCieConst.Kid];
            Throw<Exception>.If(string.IsNullOrWhiteSpace(kid), $"No Kid specified in the EntityConfiguration JWT Header: {decodedOpJwtHeader}");

            var key = entityStatement!.JWKS.Keys.FirstOrDefault(k => k.kid.Equals(kid, StringComparison.InvariantCultureIgnoreCase));
            Throw<Exception>.If(key is null, $"No key found with kid {kid} in the EntityStatement at url {url}: {decodedEsJwt}");

            RSA publicKey = _cryptoService.GetRSAPublicKey(key!);
            var decodedOpJwt = _cryptoService.DecodeJWT(opJwt);
            Throw<Exception>.If(string.IsNullOrWhiteSpace(decodedOpJwt) || !decodedOpJwt.Equals(_cryptoService.ValidateJWTSignature(opJwt, publicKey)),
                $"Invalid Signature for the EntityConfiguration JWT verified with the EntityStatement at url {url}: EntityConfiguration JWT {opJwt} - EntityStatement JWT {esJwt}");

            return entityStatement;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return default;
        }
    }
}

