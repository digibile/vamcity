using System;
using System.Security.Claims;
using System.Threading.Tasks;
using LoanSystem.Data;
using LoanSystem.Data.Entities;
using LoanSystem.Services.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LoanSystem.Site.Helpers.Middlewares;

public class VisitorMiddleware
{
    private readonly RequestDelegate _next;

    private readonly UserManager _userManager;

    private readonly AppDbContext _dbContext;

    private readonly IMemoryCache _memoryCache;

    public VisitorMiddleware(RequestDelegate next, UserManager userManager, AppDbContext dbContext, IMemoryCache memoryCache)
    {
        _next = next;
        _userManager = userManager;
        _dbContext = dbContext;
        _memoryCache = memoryCache;
    }

    public async Task Invoke(HttpContext context)
    {
        await _next(context);
        DateTime orCreate = _memoryCache.GetOrCreate("lastUpdate", (ICacheEntry cacheEntry) => DateTime.MinValue);
        if (!(orCreate == DateTime.MinValue) && !((DateTime.Now - orCreate).TotalMinutes >= 5.0))
        {
            return;
        }
        ClaimsPrincipal user = context.User;
        if (user == null || user.Identity.Name == null || user.HasClaim((Claim x) => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/anonymous"))
        {
            return;
        }
        string username = _userManager.NormalizeName(user.Identity.Name);
        User user2 = await _dbContext.Users.FirstOrDefaultAsync((User x) => x.NormalizedUserName == username);
        if (user2 != null)
        {
            user2.LastLoginDate2 = user2.LastLoginDate;
            user2.LastLoginDate = DateTime.Now;
            _dbContext.Update(user2);
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
            }
            _memoryCache.Set("lastUpdate", DateTime.Now);
        }
    }
}
