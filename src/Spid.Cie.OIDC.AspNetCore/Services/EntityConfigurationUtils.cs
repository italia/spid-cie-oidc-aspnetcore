using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json.Linq;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

interface IEntityConfigurationUtils
{
  Task<(T? conf, string? decodedJwt, string? jwt)> ValidateAndDecodeEntityConfiguration<T>(string? url)
  where T : FederationEntityConfiguration;

  (T? conf, string? decodedJwt, string? jwt) ParseEntityConfiguration<T>(string metadataAddress, string jwt)
  where T : FederationEntityConfiguration;
}

class EntityConfigurationUtils : IEntityConfigurationUtils
{
    readonly HttpClient _httpClient;
    readonly ILogPersister _logPersister;
    readonly ICryptoService _cryptoService;
    readonly ILogger<EntityConfigurationUtils> _logger;

    public EntityConfigurationUtils(IHttpClientFactory httpClientFactory, ICryptoService cryptoService,
                                ILogPersister logPersister, ILogger<EntityConfigurationUtils> logger)
    {
        _logger = logger;
        _logPersister = logPersister;
        _cryptoService = cryptoService;
        _httpClient = httpClientFactory.CreateClient(SpidCieConst.BackchannelClientName);
    }


    public async Task<(T? conf, string? decodedJwt, string? jwt)> ValidateAndDecodeEntityConfiguration<T>(string? url)
        where T : FederationEntityConfiguration
    {
        try
        {
            Throw<Exception>.If(string.IsNullOrWhiteSpace(url), "Url parameter is not defined");

            var metadataAddress = $"{url.EnsureTrailingSlash()!}{SpidCieConst.EntityConfigurationPath}";
            var jwt = await _httpClient.GetStringAsync(metadataAddress);
            Throw<Exception>.If(string.IsNullOrWhiteSpace(jwt), $"EntityConfiguration JWT not retrieved from url {metadataAddress}");

            await _logPersister.LogGetEntityConfiguration(metadataAddress, jwt);

            return ParseEntityConfiguration<T>(metadataAddress,jwt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return default;
        }
    }

    public (T? conf, string? decodedJwt, string? jwt) ParseEntityConfiguration<T>(string metadataAddress, string jwt)
        where T : FederationEntityConfiguration
    {
        try
        {
            var decodedJwt = _cryptoService.DecodeJWT(jwt);
            Throw<Exception>.If(string.IsNullOrWhiteSpace(decodedJwt), $"Invalid EntityConfiguration JWT for url {metadataAddress}: {jwt}");

            T conf = System.Text.Json.JsonSerializer.Deserialize<T>(decodedJwt);
            Throw<Exception>.If(conf is null, $"Invalid Decoded EntityConfiguration JWT for url {metadataAddress}: {decodedJwt}");

            var decodedJwtHeader = _cryptoService.DecodeJWTHeader(jwt);
            Throw<Exception>.If(string.IsNullOrWhiteSpace(decodedJwtHeader), $"Invalid EntityConfiguration JWT Header for url {metadataAddress}: {jwt}");

            var header = JObject.Parse(decodedJwtHeader);
            var kid = (string)header[SpidCieConst.Kid];
            Throw<Exception>.If(string.IsNullOrWhiteSpace(kid), $"No Kid specified in the EntityConfiguration JWT Header for url {metadataAddress}: {decodedJwtHeader}");

            var key = conf!.JWKS.Keys.FirstOrDefault(k => k.Kid.Equals(kid, StringComparison.InvariantCultureIgnoreCase));
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
}
