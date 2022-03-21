using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Logging;
using Spid.Cie.OIDC.AspNetCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.OpenIdFederation;

internal class TrustChainManager : ITrustChainManager
{
    private readonly HttpClient _httpClient;
    private readonly ICryptoService _cryptoService;
    private readonly ILogPersister _logPersister;
    private readonly ILogger<TrustChainManager> _logger;
    private static readonly Dictionary<string, KeyValuePair<DateTimeOffset, IdPEntityConfiguration>> _trustChainCache = new Dictionary<string, KeyValuePair<DateTimeOffset, IdPEntityConfiguration>>();
    private static readonly SemaphoreSlim _syncLock = new SemaphoreSlim(1);

    public TrustChainManager(IHttpClientFactory httpClientFactory,
        ICryptoService cryptoService,
        ILogPersister logPersister,
        ILogger<TrustChainManager> logger)
    {
        _httpClient = httpClientFactory.CreateClient(SpidCieConst.BackchannelClientName);
        _cryptoService = cryptoService;
        _logPersister = logPersister;
        _logger = logger;
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
                    opConf.Metadata.OpenIdProvider = OpenIdConnectConfiguration.Create(JObject.Parse(opDecodedJwt)["metadata"]["openid_provider"].ToString());
                    opConf.Metadata.OpenIdProvider.JsonWebKeySet = JsonWebKeySet.Create(JObject.Parse(opDecodedJwt)["metadata"]["openid_provider"]["jwks"].ToString());

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
                        (opConf, DateTimeOffset? esExpiresOn) = await GetEntityStatementAndApplyPolicy(fetchUrl, opConf, opJwt);
                        if (opConf != null && esExpiresOn.HasValue)
                        {
                            if (esExpiresOn < expiresOn)
                                expiresOn = esExpiresOn.Value;

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
            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogWarning("Url parameter is not defined");
                return default;
            }
            var metadataAddress = $"{url.EnsureTrailingSlash()}{SpidCieConst.EntityConfigurationPath}";
            var jwt = await _httpClient.GetStringAsync(metadataAddress);
            if (string.IsNullOrWhiteSpace(jwt))
            {
                _logger.LogWarning($"EntityConfiguration JWT not retrieved from url {metadataAddress}");
                return default;
            }

            await _logPersister.LogGetEntityConfiguration(metadataAddress, jwt);

            var decodedJwt = _cryptoService.DecodeJWT(jwt);
            if (string.IsNullOrWhiteSpace(decodedJwt))
            {
                _logger.LogWarning($"Invalid EntityConfiguration JWT for url {metadataAddress}: {jwt}");
                return default;
            }
            var conf = JsonSerializer.Deserialize<T>(decodedJwt);
            if (conf is null)
            {
                _logger.LogWarning($"Invalid Decoded EntityConfiguration JWT for url {metadataAddress}: {decodedJwt}");
                return default;
            }
            var decodedJwtHeader = _cryptoService.DecodeJWTHeader(jwt);
            if (string.IsNullOrWhiteSpace(decodedJwtHeader))
            {
                _logger.LogWarning($"Invalid EntityConfiguration JWT Header for url {metadataAddress}: {jwt}");
                return default;
            }
            var header = JObject.Parse(decodedJwtHeader);
            var kid = (string)header[SpidCieConst.Kid];
            if (string.IsNullOrWhiteSpace(kid))
            {
                _logger.LogWarning($"No Kid specified in the EntityConfiguration JWT Header for url {metadataAddress}: {decodedJwtHeader}");
                return default;
            }
            var key = conf.JWKS.Keys.FirstOrDefault(k => k.kid.Equals(kid, System.StringComparison.InvariantCultureIgnoreCase));
            if (key is null)
            {
                _logger.LogWarning($"No key found with kid {kid} for url {metadataAddress}: {decodedJwtHeader}");
                return default;
            }
            RSA publicKey = _cryptoService.GetRSAPublicKey(key);
            if (!decodedJwt.Equals(_cryptoService.ValidateJWTSignature(jwt, publicKey)))
            {
                _logger.LogWarning($"Invalid Signature for the EntityConfiguration JWT retrieved at the url {metadataAddress}: {decodedJwtHeader}");
                return default;
            }

            return (conf, decodedJwt, jwt);
        }
        catch (System.Exception ex)
        {
            _logger.LogWarning(ex, $"An error occurred while retrieving the EntityConfiguration at the url {url}: {ex.Message}");
            return default;
        }
    }

    private async Task<(IdPEntityConfiguration? conf, DateTimeOffset? expiresOn)> GetEntityStatementAndApplyPolicy(string? url, IdPEntityConfiguration? opConf, string opJwt)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogWarning("Url parameter is not defined");
                return default;
            }
            var esJwt = await _httpClient.GetStringAsync(url);
            if (string.IsNullOrWhiteSpace(esJwt))
            {
                _logger.LogWarning($"EntityStatement JWT not retrieved from url {url}");
                return default;
            }

            await _logPersister.LogGetEntityStatement(url, esJwt);

            var decodedEsJwt = _cryptoService.DecodeJWT(esJwt);
            if (string.IsNullOrWhiteSpace(decodedEsJwt))
            {
                _logger.LogWarning($"Invalid EntityStatement JWT for url {url}: {esJwt}");
                return default;
            }
            var entityStatement = JsonSerializer.Deserialize<EntityStatement>(decodedEsJwt);
            if (entityStatement is null)
            {
                _logger.LogWarning($"Invalid Decoded EntityStatement JWT for url {url}: {decodedEsJwt}");
                return default;
            }
            var decodedOpJwtHeader = _cryptoService.DecodeJWTHeader(opJwt);
            if (string.IsNullOrWhiteSpace(decodedOpJwtHeader))
            {
                _logger.LogWarning($"Invalid EntityConfiguration JWT Header: {opJwt}");
                return default;
            }
            var opHeader = JObject.Parse(decodedOpJwtHeader);
            var kid = (string)opHeader[SpidCieConst.Kid];
            if (string.IsNullOrWhiteSpace(kid))
            {
                _logger.LogWarning($"No Kid specified in the EntityConfiguration JWT Header: {decodedOpJwtHeader}");
                return default;
            }
            var key = entityStatement.JWKS.Keys.FirstOrDefault(k => k.kid.Equals(kid, System.StringComparison.InvariantCultureIgnoreCase));
            if (key is null)
            {
                _logger.LogWarning($"No key found with kid {kid} in the EntityStatement at url {url}: {decodedEsJwt}");
                return default;
            }
            RSA publicKey = _cryptoService.GetRSAPublicKey(key);
            var decodedOpJwt = _cryptoService.DecodeJWT(opJwt);
            if (string.IsNullOrWhiteSpace(decodedOpJwt)
                || !decodedOpJwt.Equals(_cryptoService.ValidateJWTSignature(opJwt, publicKey)))
            {
                _logger.LogWarning($"Invalid Signature for the EntityConfiguration JWT verified with the EntityStatement at url {url}: EntityConfiguration JWT {opJwt} - EntityStatement JWT {esJwt}");
                return default;
            }
            // Apply policy

            return (opConf, entityStatement.ExpiresOn);
        }
        catch (System.Exception ex)
        {
            _logger.LogWarning(ex, $"An error occurred while retrieving the EntityStatement at the url {url}: {ex.Message}");
            return default;
        }
    }
}

