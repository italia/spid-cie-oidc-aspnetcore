namespace Spid.Cie.OIDC.AspNetCore.Helpers;

static class StringHelpers
{
    public static string EnsureTrailingSlash(this string url)
    {
        if (!url.EndsWith("/"))
            return url + "/";

        return url;
    }

    public static string RemoveLeadingSlash(this string url)
    {
        if (url.StartsWith("/"))
            url = url.Substring(1);

        return url;
    }

    public static string RemoveTrailingSlash(this string url)
    {
        if (url.EndsWith("/"))
            url = url.Substring(0, url.Length - 1);

        return url;
    }
}