using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace LoanSystemAvizhe.Core;

public class CurrentUserService : ICurrentUserService
{
	private const string HEADER_KEY_PREFIX = "JETAQSAAT_APP_";

	public const string CUSTOMER_HEADER_KEY = "JETAQSAAT_APP_CID";

	public const string USER_HEADER_KEY = "JETAQSAAT_APP_UID";

	public const string OWNER_HEADER_KEY = "JETAQSAAT_APP_OWNER";

	private readonly IHttpContextAccessor _httpContextAccessor;

	private Guid? _customerId;

	private long? _userId;

	private bool? _isOwner;

	public static string Customer_header_key => "JETAQSAAT_APP_CID";

	public Guid? CustomerId
	{
		get
		{
			return GetCustomerId();
		}
		set
		{
			if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey(Customer_header_key))
			{
				_httpContextAccessor.HttpContext.Request.Headers[Customer_header_key] = value.ToString();
			}
			else
			{
				_httpContextAccessor.HttpContext.Request.Headers.Add(Customer_header_key, value.ToString());
			}
		}
	}

	public long? UserId
	{
		get
		{
			return GetUserId();
		}
		set
		{
			if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey("JETAQSAAT_APP_UID"))
			{
				_httpContextAccessor.HttpContext.Request.Headers["JETAQSAAT_APP_UID"] = value.ToString();
			}
			else
			{
				_httpContextAccessor.HttpContext.Request.Headers.Add("JETAQSAAT_APP_UID", value.ToString());
			}
		}
	}

	public bool? IsOwner => GetIsOwner();

	public bool IsAuthenticated => UserId.HasValue;

	public CurrentUserService(IHttpContextAccessor httpContextAccessor)
	{
		_httpContextAccessor = httpContextAccessor;
	}

	public bool IsInRole(string role)
	{
		return _httpContextAccessor.HttpContext.User?.IsInRole(role) ?? false;
	}

	public bool TrySetCustomerIdFromToken()
	{
		Claim claim = _httpContextAccessor.HttpContext.User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/userdata");
		if (claim == null || string.IsNullOrEmpty(claim.Value))
		{
			return false;
		}
		try
		{
			IDictionary<string, object> dictionary = JsonSerializer.Deserialize<IDictionary<string, object>>(claim.Value);
			if (!Guid.TryParse(((JsonElement)dictionary["customer"]).GetProperty("id").ToString(), out var result))
			{
				return false;
			}
			if (!long.TryParse(dictionary.FirstOrDefault((KeyValuePair<string, object> x) => x.Key == "id").Value.ToString(), out var result2))
			{
				return false;
			}
			CustomerId = result;
			UserId = result2;
			return true;
		}
		catch
		{
			return false;
		}
	}

	private Guid? GetCustomerId()
	{
		if (_customerId.HasValue)
		{
			return _customerId;
		}

		var httpContext = _httpContextAccessor.HttpContext;
		if (httpContext == null)
		{
			return null;
		}

		// Only a privileged SuperAdmin may select a tenant through the SPA header.
		// For ordinary accounts, the signed token remains the source of tenant identity.
		if (httpContext.User?.IsInRole("SuperAdmin") == true &&
			httpContext.Request.Headers.TryGetValue(CUSTOMER_HEADER_KEY, out var adminHeader) &&
			Guid.TryParse(adminHeader, out var selectedCustomerId))
		{
			_customerId = selectedCustomerId;
			return _customerId;
		}

		// Stale/empty SPA headers must not override a signed claim and silently
		// filter every tenant-scoped database query.
		Claim tenantClaim = httpContext.User?.FindFirst(CUSTOMER_HEADER_KEY);
		if (tenantClaim != null && Guid.TryParse(tenantClaim.Value, out var tokenCustomerId))
		{
			_customerId = tokenCustomerId;
			return _customerId;
		}

		// Older tokens may only contain the serialized userData claim.
		Claim userDataClaim = httpContext.User?.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/userdata");
		if (!string.IsNullOrWhiteSpace(userDataClaim?.Value))
		{
			try
			{
				using JsonDocument document = JsonDocument.Parse(userDataClaim.Value);
				if (document.RootElement.TryGetProperty("customer", out var customer) &&
					customer.ValueKind == JsonValueKind.Object &&
					customer.TryGetProperty("id", out var id) &&
					Guid.TryParse(id.ToString(), out var parsedCustomerId))
				{
					_customerId = parsedCustomerId;
					return _customerId;
				}
			}
			catch (JsonException)
			{
				// Fall through to the safe legacy-header path below.
			}
		}

		// Never accept a client-supplied tenant header as identity for an
		// authenticated account when the signed token has no tenant claim.
		if (httpContext.User?.Identity?.IsAuthenticated == true)
		{
			return null;
		}

		if (httpContext.Request.Headers.TryGetValue(CUSTOMER_HEADER_KEY, out var value) &&
			Guid.TryParse(value, out var anonymousCustomerId))
		{
			_customerId = anonymousCustomerId;
		}
		return _customerId;
	}

	private long? GetUserId()
	{
		if (_userId.HasValue)
		{
			return _userId;
		}

		var httpContext = _httpContextAccessor.HttpContext;
		if (httpContext == null)
		{
			return null;
		}

		// The user ID must come from the signed token, not from a mutable SPA
		// header. This prevents a stale browser value from breaking user-scoped
		// queries and prevents callers from impersonating another user by header.
		Claim userClaim = httpContext.User?.FindFirst(USER_HEADER_KEY);
		if (userClaim != null && long.TryParse(userClaim.Value, out var tokenUserId))
		{
			_userId = tokenUserId;
			return _userId;
		}

		Claim userDataClaim = httpContext.User?.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/userdata");
		if (!string.IsNullOrWhiteSpace(userDataClaim?.Value))
		{
			try
			{
				using JsonDocument document = JsonDocument.Parse(userDataClaim.Value);
				if (document.RootElement.TryGetProperty("id", out var id) &&
					long.TryParse(id.ToString(), out var parsedUserId))
				{
					_userId = parsedUserId;
					return _userId;
				}
			}
			catch (JsonException)
			{
				// Fall through to the unauthenticated legacy-header path below.
			}
		}

		if (httpContext.User?.Identity?.IsAuthenticated == true)
		{
			return null;
		}

		if (httpContext.Request.Headers.TryGetValue(USER_HEADER_KEY, out var value) &&
			long.TryParse(value, out var anonymousUserId))
		{
			_userId = anonymousUserId;
		}
		return _userId;
	}

	private bool? GetIsOwner()
	{
		if (_isOwner.HasValue)
		{
			return _isOwner;
		}
		foreach (Claim claim in _httpContextAccessor.HttpContext.User.Claims)
		{
			if (claim.Type == "JETAQSAAT_APP_OWNER" && bool.TryParse(claim.Value, out var result))
			{
				_isOwner = result;
			}
		}
		return _isOwner;
	}
}
