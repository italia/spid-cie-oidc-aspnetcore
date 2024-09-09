using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services.Defaults;

class DefaultLogPersister : ILogPersister
{
    readonly ILogger<DefaultLogPersister> _logger;

    public DefaultLogPersister(ILogger<DefaultLogPersister> logger)
    {
        _logger = logger;
    }

    public Task LogGetEntityConfiguration(string metadataAddress, string jwt)
    {
        _logger.LogInformation($"LogGetEntityConfiguration from {metadataAddress}: {jwt}");
        return Task.CompletedTask;
    }

    public Task LogGetEntityStatement(string url, string esJwt)
    {
        _logger.LogInformation($"LogGetEntityStatement from {url}: {esJwt}");
        return Task.CompletedTask;
    }

    public Task LogRequest(Uri requestUri, string? content)
    {
        _logger.LogInformation($"LogRequest to {requestUri.OriginalString}: {content}");
        return Task.CompletedTask;
    }

    public Task LogResponse(Uri requestUri, HttpStatusCode statusCode, string? content)
    {
        _logger.LogInformation($"LogResponse from {requestUri.OriginalString}: {content}");
        return Task.CompletedTask;
    }
}