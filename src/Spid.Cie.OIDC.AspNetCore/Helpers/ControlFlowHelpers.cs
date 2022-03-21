using System;

namespace Spid.Cie.OIDC.AspNetCore.Helpers;

internal static class Throw<T>
    where T : Exception
{
    public static void If(bool condition, string message)
    {
        if (condition)
            throw (Exception)Activator.CreateInstance(typeof(T), message)!;
    }
}




