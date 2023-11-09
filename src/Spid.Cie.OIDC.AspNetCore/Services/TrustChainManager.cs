using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
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
    private static readonly ConcurrentDictionary<string, TrustChain> _trustChainCache = new();
    private static readonly SemaphoreSlim _syncLock = new SemaphoreSlim(1, 1);

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

    public TrustChain? GetResolvedTrustChain(string sub, string anchor)
    {
        if (_trustChainCache.ContainsKey(sub)
            && _trustChainCache[sub].TrustAnchorUsed.Equals(anchor, StringComparison.OrdinalIgnoreCase)
            && _trustChainCache[sub].ExpiresOn >= DateTimeOffset.UtcNow)
        {
            return _trustChainCache[sub];
        }
        return null;
    }

    public async Task<IdPEntityConfiguration?> BuildTrustChain(string url)
    {
        if (!_trustChainCache.ContainsKey(url) || _trustChainCache[url].ExpiresOn < DateTimeOffset.UtcNow)
        {
            if (!await _syncLock.WaitAsync(TimeSpan.FromSeconds(10)))
            {
                _logger.LogWarning("TrustChain cache Sync Lock expired.");
                return default;
            }
            if (!_trustChainCache.ContainsKey(url) || _trustChainCache[url].ExpiresOn < DateTimeOffset.UtcNow)
            {
                try
                {
                    List<string> trustChain = new();
                    string? trustAnchorUsed = default;

                    (IdPEntityConfiguration? opConf, string? decodedOPJwt, string? opJwt) = await ValidateAndDecodeEntityConfiguration<IdPEntityConfiguration>(url);
                    if (opConf is null || opJwt is null || opConf.ExpiresOn < DateTime.UtcNow)
                    {
                        _logger.LogWarning($"EntityConfiguration not retrieved for OP {url}");
                        return default;
                    }

                    DateTimeOffset expiresOn = opConf.ExpiresOn;

                    bool opValidated = false;
                    foreach (var authorityHint in opConf.AuthorityHints)
                    {
                        trustChain.Clear();

                        (TAEntityConfiguration? taConf, string? decodedTAJwt, string? taJwt) = await ValidateAndDecodeEntityConfiguration<TAEntityConfiguration>(authorityHint);
                        if (taConf is null || taJwt is null || taConf.ExpiresOn < DateTime.UtcNow)
                        {
                            _logger.LogWarning($"EntityConfiguration not retrieved for TA {authorityHint}");
                            continue;
                        }

                        trustChain.Add(taJwt);

                        if (taConf.ExpiresOn < expiresOn)
                            expiresOn = taConf.ExpiresOn;

                        var fetchUrl = $"{taConf.Metadata.FederationEntity.FederationFetchEndpoint}?sub={url}";
                        (EntityStatement? entityStatement, string? decodedEsJwt, string? esJwt) = await GetAndValidateEntityStatement(fetchUrl, opJwt);

                        if (entityStatement is null || esJwt is null || entityStatement.ExpiresOn < DateTime.UtcNow)
                        {
                            _logger.LogWarning($"EntityStatement not retrieved for OP {url}");
                            continue;
                        }

                        trustChain.Add(esJwt);

                        var esExpiresOn = entityStatement.ExpiresOn;

                        // Apply policy
                        opConf!.Metadata!.OpenIdProvider = _metadataPolicyHandler.ApplyMetadataPolicy(decodedOPJwt!, entityStatement.MetadataPolicy.ToJsonString());

                        if (opConf!.Metadata!.OpenIdProvider is not null)
                        {
                            if (!string.IsNullOrWhiteSpace(opConf!.Metadata!.OpenIdProvider.JwksUri))
                            {
                                var keys = await _httpClient.GetStringAsync(opConf!.Metadata!.OpenIdProvider.JwksUri);
                                if (!string.IsNullOrWhiteSpace(keys))
                                {
                                    opConf!.Metadata!.OpenIdProvider.JsonWebKeySet = JsonConvert.DeserializeObject<JsonWebKeySet>(keys);
                                }
                            }
                            else if (!string.IsNullOrWhiteSpace(JObject.Parse(decodedOPJwt)["metadata"]["openid_provider"]["jwks"].ToString()))
                            {
                                opConf!.Metadata!.OpenIdProvider.JsonWebKeySet = JsonWebKeySet.Create(JObject.Parse(decodedOPJwt)["metadata"]["openid_provider"]["jwks"].ToString());
                            }
                            if (opConf!.Metadata!.OpenIdProvider.JsonWebKeySet is null)
                            {
                                _logger.LogWarning($"No jwks found for the OP {url} validated by the authorityHint {authorityHint}");
                                continue;
                            }

                            foreach (SecurityKey key in opConf!.Metadata!.OpenIdProvider.JsonWebKeySet.GetSigningKeys())
                            {
                                opConf!.Metadata!.OpenIdProvider.SigningKeys.Add(key);
                            }
                        }


                        if (opConf is not null && opConf.Metadata?.OpenIdProvider is not null)
                        {
                            trustChain.Add(opJwt);

                            expiresOn = esExpiresOn < expiresOn ? esExpiresOn : expiresOn;
                            opValidated = true;
                            trustAnchorUsed = authorityHint;
                            break;
                        }
                    }
                    if (opValidated && opConf is not null && trustAnchorUsed is not null)
                    {
                        _trustChainCache.AddOrUpdate(url, new TrustChain()
                        {
                            ExpiresOn = expiresOn,
                            OpConf = opConf,
                            Chain = trustChain,
                            TrustAnchorUsed = trustAnchorUsed
                        }, (oldValue, newValue) => newValue);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                    throw;
                }
                finally
                {
                    _syncLock.Release();
                }
            }
        }
        return _trustChainCache.ContainsKey(url) && _trustChainCache[url].ExpiresOn.Add(SpidCieConst.TrustChainExpirationGracePeriod) > DateTimeOffset.UtcNow
            ? _trustChainCache[url].OpConf
            : null;
    }

    private async Task<(T? conf, string? decodedJwt, string? jwt)> ValidateAndDecodeEntityConfiguration<T>(string? url)
        where T : FederationEntityConfiguration
    {
        try
        {
            Throw<Exception>.If(string.IsNullOrWhiteSpace(url), "Url parameter is not defined");

            var metadataAddress = $"{url.EnsureTrailingSlash()!}{SpidCieConst.EntityConfigurationPath}";
            var jwt = await _httpClient.GetStringAsync(metadataAddress);
            Throw<Exception>.If(string.IsNullOrWhiteSpace(jwt), $"EntityConfiguration JWT not retrieved from url {metadataAddress}");

            await _logPersister.LogGetEntityConfiguration(metadataAddress, jwt);

            var decodedJwt = _cryptoService.DecodeJWT(jwt);
            Throw<Exception>.If(string.IsNullOrWhiteSpace(decodedJwt), $"Invalid EntityConfiguration JWT for url {metadataAddress}: {jwt}");

            var conf = System.Text.Json.JsonSerializer.Deserialize<T>(decodedJwt);
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
            _logger.LogError(ex, ex.Message);
            return default;
        }
    }

    private async Task<(EntityStatement?, string? decodedEsJwt, string esJwt)> GetAndValidateEntityStatement(string? url, string opJwt)
    {
        try
        {
            Throw<Exception>.If(string.IsNullOrWhiteSpace(url), "Url parameter is not defined");

            var esJwt = await _httpClient.GetStringAsync(url);
            Throw<Exception>.If(string.IsNullOrWhiteSpace(esJwt), $"EntityStatement JWT not retrieved from url {url}");

            await _logPersister.LogGetEntityStatement(url!, esJwt);

            var decodedEsJwt = _cryptoService.DecodeJWT(esJwt);
            Throw<Exception>.If(string.IsNullOrWhiteSpace(decodedEsJwt), $"Invalid EntityStatement JWT for url {url}: {esJwt}");

            var entityStatement = System.Text.Json.JsonSerializer.Deserialize<EntityStatement>(decodedEsJwt);
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
            Throw<Exception>.If(!decodedOpJwt.Equals(_cryptoService.ValidateJWTSignature(opJwt, publicKey)),
                $"Invalid Signature for the EntityConfiguration JWT verified with the EntityStatement at url {url}: EntityConfiguration JWT {opJwt} - EntityStatement JWT {esJwt}");

            return (entityStatement, decodedEsJwt, esJwt);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, ex.Message);
            return default;
        }
    }
}

