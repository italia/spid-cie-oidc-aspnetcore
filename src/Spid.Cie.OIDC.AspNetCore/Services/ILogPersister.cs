using System;
using System.Net;
using System.Threading.Tasks;

namespace Spid.Cie.OIDC.AspNetCore.Services;

public interface ILogPersister
{
    Task LogGetEntityConfiguration(string metadataAddress, string jwt);
    Task LogGetEntityStatement(string url, string esJwt);
    Task LogRequest(Uri requestUri, string? content);
    Task LogResponse(Uri requestUri, HttpStatusCode statusCode, string? content);
}