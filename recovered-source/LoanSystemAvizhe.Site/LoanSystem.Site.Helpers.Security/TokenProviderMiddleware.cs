using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using LoanSystem.Site.Helpers.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LoanSystem.Site.Helpers.Security;

public class TokenProviderMiddleware
{
    private readonly RequestDelegate _next;

    private readonly TokenProviderOptions _options;

    private readonly ILogger _logger;

    private readonly JsonSerializerOptions _serializerSettings;

    public TokenProviderMiddleware(RequestDelegate next, IOptions<TokenProviderOptions> options, ILoggerFactory loggerFactory)
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<TokenProviderMiddleware>();
        _options = options.Value;
        ThrowIfInvalidOptions(_options);
        _serializerSettings = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public Task Invoke(HttpContext context)
    {
        if (!context.Request.Path.Equals(_options.Path, StringComparison.OrdinalIgnoreCase))
        {
            return _next(context);
        }
        if (!context.Request.Method.Equals("POST") || !context.Request.HasFormContentType)
        {
            context.Response.StatusCode = 400;
            return context.Response.WriteAsync("Bad request.");
        }
        _logger.LogInformation("Handling request: " + context.Request.Path);
        return GenerateToken(context);
    }

    private async Task MakeIdentityResponse(HttpResponse response, bool isSuccess, string msg, JwtToken data)
    {
        ResponseState<JwtToken> responseState = new ResponseState<JwtToken>();
        response.StatusCode = 200;
        response.Headers.Add("Content-type", "application/json");
        responseState.Success = isSuccess;
        responseState.Message = msg;
        responseState.Data = data;
        string text = JsonSerializer.Serialize(responseState, _serializerSettings);
        await response.WriteAsync(text);
    }

    private async Task GenerateToken(HttpContext context)
    {
        string arg = context.Request.Form["username"].ToString();
        string arg2 = context.Request.Form["password"].ToString();
        new DefaultResponseState();
        ClaimsIdentity identity;
        try
        {
            identity = await _options.IdentityResolver(arg, arg2);
        }
        catch (Exception ex)
        {
            await MakeIdentityResponse(context.Response, isSuccess: false, ex.Message, null);
            return;
        }
        if (identity == null)
        {
            await MakeIdentityResponse(context.Response, isSuccess: false, "حساب کاربری با این مشخصات وجود ندارد", null);
            return;
        }
        JwtTokenService jwtTokenService = new JwtTokenService(null, null);
        JwtToken data = await jwtTokenService.GetJwtToken(identity, _options);
        await MakeIdentityResponse(context.Response, isSuccess: true, "", data);
    }

    private static void ThrowIfInvalidOptions(TokenProviderOptions options)
    {
        if (string.IsNullOrEmpty(options.Path))
        {
            throw new ArgumentNullException("Path");
        }
        if (string.IsNullOrEmpty(options.Issuer))
        {
            throw new ArgumentNullException("Issuer");
        }
        if (string.IsNullOrEmpty(options.Audience))
        {
            throw new ArgumentNullException("Audience");
        }
        if (options.Expiration == TimeSpan.Zero)
        {
            throw new ArgumentException("Must be a non-zero TimeSpan.", "Expiration");
        }
        if (options.IdentityResolver == null)
        {
            throw new ArgumentNullException("IdentityResolver");
        }
        if (options.SigningCredentials == null)
        {
            throw new ArgumentNullException("SigningCredentials");
        }
        if (options.NonceGenerator == null)
        {
            throw new ArgumentNullException("NonceGenerator");
        }
    }

    public static long ToUnixEpochDate(DateTime date)
    {
        return new DateTimeOffset(date).ToUniversalTime().ToUnixTimeSeconds();
    }
}
