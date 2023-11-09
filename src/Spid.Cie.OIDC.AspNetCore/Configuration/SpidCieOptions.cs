using Microsoft.Extensions.Configuration;
using Spid.Cie.OIDC.AspNetCore.Helpers;
using Spid.Cie.OIDC.AspNetCore.Models;
using System.Collections.Generic;

namespace Spid.Cie.OIDC.AspNetCore.Configuration;

public sealed class SpidCieOptions
{
    public bool RequestRefreshToken { get; set; } = false;

    public List<RelyingParty> RelyingParties { get; } = new List<RelyingParty>();

    public List<Aggregator> Aggregators { get; } = new List<Aggregator>();

    public List<string> SpidOPs { get; } = new List<string>();

    public List<string> CieOPs { get; } = new List<string>();


    /// <summary>
    /// Loads from configuration.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <returns></returns>
    public void LoadFromConfiguration(IConfiguration configuration)
    {
        var conf = OptionsHelpers.CreateFromConfiguration(configuration);
        RequestRefreshToken = conf.RequestRefreshToken;
        RelyingParties.AddRange(conf.RelyingParties);
        SpidOPs.AddRange(conf.SpidOPs);
        CieOPs.AddRange(conf.CieOPs);
        Aggregators.AddRange(conf.Aggregators);
    }
}
