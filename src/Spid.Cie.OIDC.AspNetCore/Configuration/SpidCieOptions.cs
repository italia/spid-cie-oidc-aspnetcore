using Microsoft.Extensions.Configuration;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using System.Collections.Generic;

namespace Spid.Cie.OIDC.AspNetCore.Configuration;

public sealed class SpidCieOptions
{
    public string TrustAnchorUrl { get; set; } = "http://127.0.0.1:8000/";

    public bool RequestRefreshToken { get; set; } = false;

    public List<RelyingParty> RelyingParties { get; set; } = new List<RelyingParty>();

    /// <summary>
    /// Loads from configuration.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <returns></returns>
    public void LoadFromConfiguration(IConfiguration configuration)
    {
        var conf = OptionsHelpers.CreateFromConfiguration(configuration);
    }
}
