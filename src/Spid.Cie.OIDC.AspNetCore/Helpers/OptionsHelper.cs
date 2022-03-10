using Microsoft.Extensions.Configuration;
using Spid.Cie.OIDC.AspNetCore.Models;

namespace Spid.Cie.OIDC.AspNetCore.Helpers;

internal static class OptionsHelper
{
    internal static SpidCieConfiguration CreateFromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection("SpidCie");
        var options = new SpidCieConfiguration();

        // TODO: read options from configuration

        return options;
    }
}
