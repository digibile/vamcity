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
		if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("JETAQSAAT_APP_CID", out var value))
		{
			if (Guid.TryParse(value, out var result))
			{
				_customerId = result;
			}
		}
		else
		{
			foreach (Claim claim in _httpContextAccessor.HttpContext.User.Claims)
			{
				if (claim.Type == "JETAQSAAT_APP_CID" && Guid.TryParse(claim.Value, out var result2))
				{
					_customerId = result2;
					break;
				}
			}
		}
		return _customerId;
	}

	private long? GetUserId()
	{
		if (_userId.HasValue)
		{
			return _userId;
		}
		if (_httpContextAccessor.HttpContext.Request.Headers.TryGetValue("JETAQSAAT_APP_UID", out var value))
		{
			if (long.TryParse(value, out var _))
			{
				_userId = long.Parse(value);
			}
		}
		else
		{
			foreach (Claim claim in _httpContextAccessor.HttpContext.User.Claims)
			{
				if (claim.Type == "JETAQSAAT_APP_UID" && long.TryParse(claim.Value, out var result2))
				{
					_userId = result2;
					break;
				}
			}
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
