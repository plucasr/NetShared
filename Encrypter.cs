using System.Security.Cryptography;
using System.Text;
using Shared.EnvVarLoader;

namespace Shared;

public static class EncryptService
{
    private static readonly string key = Env.PWD_SECRET;

    public static string Encrypt(string PlainText)
    {
        byte[] toEncryptedArray = Encoding.UTF8.GetBytes(PlainText);
        var objMD5CryptoService = MD5.Create();
        byte[] securityKeyArray = objMD5CryptoService.ComputeHash(Encoding.UTF8.GetBytes(key));
        objMD5CryptoService.Clear();
        var objTripleDESCryptoService = TripleDES.Create();

        objTripleDESCryptoService.Key = securityKeyArray;
        objTripleDESCryptoService.Mode = CipherMode.ECB;
        objTripleDESCryptoService.Padding = PaddingMode.PKCS7;

        var objCrytpoTransform = objTripleDESCryptoService.CreateEncryptor();

        byte[] resultArray = objCrytpoTransform.TransformFinalBlock(
            toEncryptedArray,
            0,
            toEncryptedArray.Length
        );

        objTripleDESCryptoService.Clear();

        return Convert.ToBase64String(resultArray, 0, resultArray.Length);
    }

    public static string Decrypt(string CipherText)
    {
        byte[] toEncryptArray = Convert.FromBase64String(CipherText);
        var objMD5CryptoService = MD5.Create();

        byte[] securityKeyArray = objMD5CryptoService.ComputeHash(Encoding.UTF8.GetBytes(key));
        objMD5CryptoService.Clear();

        var objTripleDESCryptoService = TripleDES.Create();
        objTripleDESCryptoService.Key = securityKeyArray;
        objTripleDESCryptoService.Mode = CipherMode.ECB;
        objTripleDESCryptoService.Padding = PaddingMode.PKCS7;

        var objCrytpoTransform = objTripleDESCryptoService.CreateDecryptor();
        byte[] resultArray = objCrytpoTransform.TransformFinalBlock(
            toEncryptArray,
            0,
            toEncryptArray.Length
        );
        objTripleDESCryptoService.Clear();

        return Encoding.UTF8.GetString(resultArray);
    }

    public static string ComputeMd5Hash(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
