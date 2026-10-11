namespace NikanBase.Services.Security;

public interface IEncryptionService
{
	string CreatePasswordHash(string password);

	string CreateHash(byte[] data);

	bool VerifyPassword(string password, string hash);

	string EncryptTripleDes(string input, string key);

	string DecryptTripleDes(string input, string key);

	string EncryptAES(string plainText, string key);

	string DecryptAES(string encryptedPassword, string key);

	string DigibileSimpleEncrypt(string input, string salt);

	string DigibileSimpleDecrypt(string ciphertext);

	string CreateMD5(string input);
}
