using Microsoft.Extensions.DependencyInjection;
using System;

namespace Spid.Cie.OIDC.AspNetCore.Extensions;

internal class SpidCieOIDCBuilder : ISpidCieOIDCBuilder
{
    public SpidCieOIDCBuilder(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public IServiceCollection Services { get; }
}
