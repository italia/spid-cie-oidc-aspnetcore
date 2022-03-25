using Newtonsoft.Json.Linq;
using System;

namespace Spid.Cie.OIDC.AspNetCore.Helpers;
internal static class JsonExtensions
{
    public static bool IsNullOrEmpty(this JToken token)
    {
        return (token == null)
            || (token.Type == JTokenType.Null)
            || (!token.HasValues)
            || (token.ToString() == String.Empty);
    }
}
