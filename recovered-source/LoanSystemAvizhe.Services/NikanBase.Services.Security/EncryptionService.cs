using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace NikanBase.Services.Security;

public class EncryptionService : IEncryptionService
{
	public string CreateHash(byte[] data)
	{
		using SHA256 sHA = SHA256.Create();
		byte[] array = sHA.ComputeHash(data);
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	public string CreatePasswordHash(string password)
	{
		return CreateHash(Encoding.UTF8.GetBytes(password));
	}

	public bool VerifyPassword(string password, string hash)
	{
		string x = CreatePasswordHash(password);
		return StringComparer.OrdinalIgnoreCase.Compare(x, hash) == 0;
	}

	public string EncryptTripleDes(string input, string base64key)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(input);
		SymmetricAlgorithm? symmetricAlgorithm = SymmetricAlgorithm.Create("TripleDes");
		symmetricAlgorithm.Mode = CipherMode.ECB;
		symmetricAlgorithm.Padding = PaddingMode.PKCS7;
		return Convert.ToBase64String(symmetricAlgorithm.CreateEncryptor(Convert.FromBase64String(base64key), new byte[8]).TransformFinalBlock(bytes, 0, bytes.Length));
	}

	public string DecryptTripleDes(string ciphertext, string base64key)
	{
		using TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		byte[] key = Convert.FromBase64String(base64key);
		tripleDESCryptoServiceProvider.Key = key;
		byte[] iV = new byte[8] { 18, 52, 86, 120, 144, 171, 205, 239 };
		tripleDESCryptoServiceProvider.IV = iV;
		tripleDESCryptoServiceProvider.Mode = CipherMode.ECB;
		using ICryptoTransform cryptoTransform = tripleDESCryptoServiceProvider.CreateDecryptor();
		byte[] array = Convert.FromBase64String(ciphertext);
		byte[] bytes = cryptoTransform.TransformFinalBlock(array, 0, array.Length);
		return Encoding.UTF8.GetString(bytes);
	}

	public string DigibileSimpleEncrypt(string input, string salt)
	{
		string s = salt + input;
		return Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
	}

	public string DigibileSimpleDecrypt(string ciphertext)
	{
		return Encoding.UTF8.GetString(Convert.FromBase64String(ciphertext)).Substring(4);
	}

	public string EncryptAES(string password, string encryptionKey)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(encryptionKey);
		if (bytes.Length != 32)
		{
			throw new ArgumentException("Key must be 32 bytes for AES-256.");
		}
		using Aes aes = Aes.Create();
		aes.Key = bytes;
		aes.GenerateIV();
		using ICryptoTransform transform = aes.CreateEncryptor(aes.Key, aes.IV);
		using MemoryStream memoryStream = new MemoryStream();
		memoryStream.Write(aes.IV, 0, aes.IV.Length);
		using (CryptoStream stream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
		{
			using StreamWriter streamWriter = new StreamWriter(stream);
			streamWriter.Write(password);
		}
		return Convert.ToBase64String(memoryStream.ToArray());
	}

	public string DecryptAES(string encryptedPassword, string encryptionKey)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(encryptionKey);
		if (bytes.Length != 32)
		{
			throw new ArgumentException("Key must be 32 bytes for AES-256.");
		}
		byte[] array = Convert.FromBase64String(encryptedPassword);
		byte[] array2 = new byte[16];
		Array.Copy(array, 0, array2, 0, array2.Length);
		byte[] array3 = new byte[array.Length - array2.Length];
		Array.Copy(array, array2.Length, array3, 0, array3.Length);
		using Aes aes = Aes.Create();
		aes.Key = bytes;
		aes.IV = array2;
		using ICryptoTransform transform = aes.CreateDecryptor(aes.Key, aes.IV);
		using MemoryStream stream = new MemoryStream(array3);
		using CryptoStream stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read);
		using StreamReader streamReader = new StreamReader(stream2);
		return streamReader.ReadToEnd();
	}

	public string CreateMD5(string input)
	{
		using MD5 mD = MD5.Create();
		byte[] bytes = Encoding.ASCII.GetBytes(input);
		return Convert.ToHexString(mD.ComputeHash(bytes)).ToLower();
	}
}
