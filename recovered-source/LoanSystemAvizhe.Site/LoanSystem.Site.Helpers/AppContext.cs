using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using LoanSystem.Core.Configuration;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Base;
using LoanSystem.Services.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Helpers;

public class AppContext : IAppContext
{
    private readonly IHttpContextAccessor _contextAccessor;

    private readonly UserManager _userManager;

    private readonly IOptions<AppConfig> _appConfig;

    private User cachedUser;

    public User User
    {
        get
        {
            if (cachedUser == null)
            {
                cachedUser = Task.Run(() => GetUser()).Result;
            }
            return cachedUser;
        }
    }

    public AppContext(IHttpContextAccessor contextAccessor, UserManager userManager, IOptions<AppConfig> appConfig)
    {
        _contextAccessor = contextAccessor;
        _userManager = userManager;
        _appConfig = appConfig;
    }

    public void ClearCache()
    {
        cachedUser = null;
    }

    public void EnsureGuestCookieRemoved()
    {
        if (_contextAccessor.HttpContext.Request.Cookies.ContainsKey(".NikanBase.GuestUser"))
        {
            _contextAccessor.HttpContext.Response.Cookies.Delete(".NikanBase.GuestUser");
        }
    }

    public bool TryGetGuestUserId(out long guestId)
    {
        if (_contextAccessor.HttpContext.Request.Cookies.TryGetValue(".NikanBase.GuestUser", out string value) && long.TryParse(value, out guestId))
        {
            return true;
        }
        guestId = 0L;
        return false;
    }

    public string GetSystemCurrency()
    {
        return _appConfig.Value.Currency;
    }

    private async Task<User> GetUser()
    {
        if (cachedUser != null)
        {
            return cachedUser;
        }
        if (_contextAccessor.HttpContext == null)
        {
            return null;
        }
        ClaimsPrincipal principal = _contextAccessor.HttpContext.User;
        if (principal != null)
        {
            User user = await _userManager.Users.FirstOrDefaultAsync((User x) => x.UserName == principal.Identity.Name);
            if (user != null)
            {
                await _userManager.ConvertUserRoleToRole(new List<User> { user });
                cachedUser = user;
                return user;
            }
        }
        return null;
    }

    private string GenerateUsername(int maxLength)
    {
        RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
        byte[] array = new byte[maxLength];
        randomNumberGenerator.GetBytes(array);
        string text = Convert.ToBase64String(array);
        string text2 = string.Empty;
        string text3 = text;
        for (int i = 0; i < text3.Length; i++)
        {
            char c = text3[i];
            if (char.IsLetterOrDigit(c))
            {
                text2 += c;
            }
        }
        return text2;
    }
}
