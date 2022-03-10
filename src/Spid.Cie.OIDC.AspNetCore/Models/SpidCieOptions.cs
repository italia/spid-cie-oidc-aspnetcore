using Microsoft.Extensions.Configuration;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using System.Collections.Generic;

namespace Spid.Cie.OIDC.AspNetCore.Models;

public sealed class SpidCieOptions
{
    public List<RelyingParty> RelyingParties { get; set; } = new List<RelyingParty>();

    /// <summary>
    /// Loads from configuration.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <returns></returns>
    public void LoadFromConfiguration(IConfiguration configuration)
    {
        var conf = OptionsHelper.CreateFromConfiguration(configuration);
    }
}
