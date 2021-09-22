namespace Steganography;

using System.Security.Cryptography;
using System.Text;

public class AESEncrypt
{
    private static Aes CreateAES(string key)
    {
        var a = Aes.Create();
        using var r = new Rfc2898DeriveBytes(key, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
        a.Key = r.GetBytes(a.BlockSize / 8);
        a.IV = new byte[a.BlockSize / 8];
        a.Padding = PaddingMode.PKCS7;
        a.Mode = CipherMode.CBC;
        return a;
    }

    public static string EncryptString(string text, string password)
    {
        byte[] textBytes = Encoding.UTF8.GetBytes(text);
        var stream = new MemoryStream();
        using var aes = CreateAES(password);
        using var crypt = new CryptoStream(stream, aes.CreateEncryptor(), CryptoStreamMode.Write);
        crypt.Write(textBytes, 0, textBytes.Length);
        crypt.FlushFinalBlock();
        return Convert.ToBase64String(stream.ToArray());
    }

    public static string DecryptString(string text, string password)
    {
        byte[] textBytes = Convert.FromBase64String(text);
        var stream = new MemoryStream();
        var aes = CreateAES(password);
        var crypt = new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Write);
        crypt.Write(textBytes, 0, textBytes.Length);
        crypt.FlushFinalBlock();
        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
