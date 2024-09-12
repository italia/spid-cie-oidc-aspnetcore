using Microsoft.Extensions.DependencyInjection;

namespace Spid.Cie.OIDC.AspNetCore.Extensions;

public interface ISpidCieOIDCBuilder
{
    /// <summary>
    /// Gets the services.
    /// </summary>
    /// <value>
    /// The services.
    /// </value>
    IServiceCollection Services { get; }
}