using Microsoft.Extensions.DependencyInjection;
using System;

namespace Spid.Cie.OIDC.AspNetCore.Extensions;

class SpidCieOIDCBuilder : ISpidCieOIDCBuilder
{
    public SpidCieOIDCBuilder(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public IServiceCollection Services { get; }
}