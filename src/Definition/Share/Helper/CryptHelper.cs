using System.Security.Cryptography;

namespace Share.Helper;

/// <summary>
/// hash加密
/// </summary>
public class CryptHelper
{
    public static string HashString(string str, HashType type = HashType.MD5)
    {
        byte[] bytes = HashData(str, type);
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }

    private static byte[] HashData(string str, HashType type = HashType.MD5)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(str);
        return type switch
        {
            HashType.MD5 => MD5.HashData(bytes),
            HashType.SHA256 => SHA256.HashData(bytes),
            HashType.SHA512 => SHA512.HashData(bytes),
            _ => throw new NotSupportedException()
        };
    }
}
