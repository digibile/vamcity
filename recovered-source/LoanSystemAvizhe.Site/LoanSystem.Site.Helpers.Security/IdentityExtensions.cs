using System;
using System.Globalization;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;

namespace LoanSystem.Site.Helpers.Security;

public static class IdentityExtensions
{
    public static void AddErrorsFromResult(this ModelStateDictionary modelStat, IdentityResult result)
    {
        foreach (IdentityError error in result.Errors)
        {
            modelStat.AddModelError("", error.Description);
        }
    }

    public static string GatherErrors(this IdentityResult result, IStringLocalizer localizer, bool useHtmlNewLine = false)
    {
        StringBuilder stringBuilder = new StringBuilder();
        if (!result.Succeeded)
        {
            foreach (IdentityError error in result.Errors)
            {
                string value = localizer["user.msg." + error.Code].Value;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (!useHtmlNewLine)
                    {
                        stringBuilder.AppendLine(value);
                    }
                    else
                    {
                        stringBuilder.AppendLine(value + "<br/>");
                    }
                }
            }
        }
        return stringBuilder.ToString();
    }

    public static string FindFirstValue(this ClaimsIdentity identity, string claimType)
    {
        return identity?.FindFirst(claimType)?.Value;
    }

    public static string GetUserClaimValue(this IIdentity identity, string claimType)
    {
        if (!(identity is ClaimsIdentity identity2))
        {
            return null;
        }
        return identity2.FindFirstValue(claimType);
    }

    public static string GetUserFirstName(this IIdentity identity)
    {
        return identity?.GetUserClaimValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname");
    }

    public static T GetUserId<T>(this IIdentity identity) where T : IConvertible
    {
        string text = identity?.GetUserClaimValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        if (text == null)
        {
            return default;
        }
        return (T)Convert.ChangeType(text, typeof(T), CultureInfo.InvariantCulture);
    }

    public static string GetUserId(this IIdentity identity)
    {
        return identity?.GetUserClaimValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
    }

    public static string GetUserData(this IIdentity identity)
    {
        return identity?.GetUserClaimValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/userdata");
    }

    public static string GetPermissions(this IIdentity identity)
    {
        return identity?.GetUserClaimValue("permissions");
    }

    public static string GetCustomData(this IIdentity identity, string key)
    {
        return identity?.GetUserClaimValue(key);
    }

    public static string GetUserLastName(this IIdentity identity)
    {
        return identity?.GetUserClaimValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname");
    }

    public static string GetUserFullName(this IIdentity identity)
    {
        return identity.GetUserFirstName() + " " + identity.GetUserLastName();
    }

    public static string GetUserDisplayName(this IIdentity identity)
    {
        string userFullName = identity.GetUserFullName();
        if (!string.IsNullOrWhiteSpace(userFullName))
        {
            return userFullName;
        }
        return identity.GetUserName();
    }

    public static string GetUserName(this IIdentity identity)
    {
        return identity?.GetUserClaimValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
    }
}
