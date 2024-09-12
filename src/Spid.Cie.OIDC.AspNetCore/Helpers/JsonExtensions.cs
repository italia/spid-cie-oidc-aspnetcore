using Newtonsoft.Json.Linq;

namespace Spid.Cie.OIDC.AspNetCore.Helpers;

static class JsonExtensions
{
    public static bool IsNullOrWhiteSpace(this JToken token)
    {
        return (token == null)
            || (token.Type == JTokenType.Null)
            || (!token.HasValues)
            || (token.ToString() == string.Empty);
    }
}