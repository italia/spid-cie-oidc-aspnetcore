using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Spid.Cie.OIDC.AspNetCore.Models;

[ExcludeFromCodeCoverage]
internal class TrustChain
{
    public DateTimeOffset ExpiresOn { get; set; }
    public IdPEntityConfiguration OpConf { get; set; }
    public List<string> Chain { get; set; }
    public string TrustAnchorUsed { get; set; }
}
