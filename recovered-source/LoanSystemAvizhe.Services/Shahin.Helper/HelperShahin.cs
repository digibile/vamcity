using System;
using System.Security.Cryptography;
using System.Text;

namespace Shahin.Helper;

public class HelperShahin
{
	public string Sign(string httpRequestMethod, string requestURI, string jsonBody, string clientID, string ClientSecret, string timestamp, string uuid)
	{
		string text = ToHex(EncryptSHA256(jsonBody.Replace("\"", "").Replace(':', '=').Replace(" ", "")));
		string message = ToHex(EncryptSHA256(httpRequestMethod + "\n/" + requestURI + "\nx-obh-timestamp:" + timestamp + "\nx-obh-uuid:" + uuid + "\n\nx-obh-timestamp;x-obh-uuid\n" + text));
		return ToHex(GetKeyedHash(GetSignKey(clientID, ClientSecret), message));
	}

	public static string CreateUUID()
	{
		return Guid.NewGuid().ToString();
	}

	public static string CreateTimeStamp()
	{
		return ((long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds).ToString();
	}

	public static byte[] EncryptSHA256(string message)
	{
		using SHA256 sHA = SHA256.Create();
		return sHA.ComputeHash(Encoding.UTF8.GetBytes(message));
	}

	public static string ToHex(byte[] bytes)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (byte b in bytes)
		{
			stringBuilder.Append(b.ToString("x2"));
		}
		return stringBuilder.ToString().ToUpper();
	}

	public static byte[] GetKeyedHash(byte[] key, string message)
	{
		return new HMACSHA256(key).ComputeHash(Encoding.UTF8.GetBytes(message));
	}

	private byte[] GetSignKey(string clientID, string clientSecret)
	{
		return EncryptSHA256(DateTime.Now.Year + clientID + clientSecret);
	}
}
