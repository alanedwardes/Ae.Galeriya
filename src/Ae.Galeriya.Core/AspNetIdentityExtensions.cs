using System;
using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;

public static class AspNetIdentityExtensions
{
    private static string FindFirstValue(ClaimsIdentity identity, string claimType)
    {
        if (identity == null)
        {
            throw new ArgumentNullException("identity");
        }
        var claim = identity.FindFirst(claimType);
        return claim != null ? claim.Value : null;
    }

    private static T GetUserId<T>(IIdentity identity) where T : IConvertible
    {
        if (identity == null)
        {
            throw new ArgumentNullException("identity");
        }
        var ci = identity as ClaimsIdentity;
        if (ci != null)
        {
            var id = FindFirstValue(ci, ClaimTypes.NameIdentifier);
            if (id != null)
            {
                return (T)Convert.ChangeType(id, typeof(T), CultureInfo.InvariantCulture);
            }
        }
        return default(T);
    }

    public static uint GetUserId(this IIdentity identity)
    {
        return GetUserId<uint>(identity);
    }
}