global using System.Text.Json;
global using System.Text.Json.Serialization;

namespace Ater.AspNetCore.Utils;

/// <summary>
/// 提供常用加解密方法
/// </summary>
public class HashCrypto
{
    private static readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();
    private static JsonSerializerOptions JsonSerializerOptions =>
        new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
        };

    /// <summary>
    /// SHA512 encrypt
    /// </summary>
    /// <param name="value"></param>
    /// <param name="salt"></param>
    /// <returns></returns>
    public static string GeneratePwd(string value, string salt)
    {
        var valueBytes = Rfc2898DeriveBytes.Pbkdf2(
            password: value,
            salt: Encoding.UTF8.GetBytes(salt),
            iterations: 100,
            hashAlgorithm: HashAlgorithmName.SHA512,
            outputLength: 32
        );
        return Convert.ToBase64String(valueBytes);
    }

    /// <summary>
    /// 生成PAT
    /// </summary>
    /// <param name="value"></param>
    /// <param name="salt"></param>
    /// <returns></returns>
    public static string GeneratePAT(string value)
    {
        var salt = BuildSalt();
        var valueBytes = Rfc2898DeriveBytes.Pbkdf2(
            password: value,
            salt: Encoding.UTF8.GetBytes(salt),
            iterations: 100,
            hashAlgorithm: HashAlgorithmName.SHA512,
            outputLength: 32
        );
        return Convert.ToBase64String(valueBytes);
    }

    /// <summary>
    /// 验证密码
    /// </summary>
    /// <param name="value"></param>
    /// <param name="salt"></param>
    /// <param name="hash"></param>
    /// <returns></returns>
    public static bool Validate(
        string value,
        string salt,
        string hash
    )
    {
        return GeneratePwd(value, salt) == hash;
    }

    /// <summary>
    /// 生成盐
    /// </summary>
    /// <returns></returns>
    public static string BuildSalt()
    {
        var randomBytes = new byte[128 / 8];
        Rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// HMACSHA256 encrypt
    /// </summary>
    /// <param name="key"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string HMACSHA256(string key, string content)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var valueBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToBase64String(valueBytes);
    }

    /// <summary>
    /// 字符串md5值
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string Md5Hash(string str)
    {
        return HashString(str);
    }

    /// <summary>
    /// 字符串hash值
    /// </summary>
    /// <param name="str"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string HashString(string str, HashType type = HashType.MD5)
    {
        var bytes = HashData(str, type);

        return Convert.ToHexStringLower(bytes);
    }

    private static byte[] HashData(string str, HashType type = HashType.MD5)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        return type switch
        {
            HashType.MD5 => MD5.HashData(bytes),
            HashType.SHA256 => SHA256.HashData(bytes),
            HashType.SHA512 => SHA512.HashData(bytes),
            _ => throw new NotSupportedException(),
        };
    }

    /// <summary>
    /// 某文件的md5值
    /// </summary>
    /// <param name="stream">file stream</param>
    /// <returns></returns>
    public static string Md5FileHash(Stream stream)
    {
        using var md5 = MD5.Create();
        var data = md5.ComputeHash(stream);
        StringBuilder sBuilder = new();

        foreach (var b in data)
        {
            _ = sBuilder.Append(b.ToString("x2"));
        }
        return sBuilder.ToString();
    }

    /// <summary>
    /// 生成随机数
    /// </summary>
    /// <param name="length"></param>
    /// <param name="useNum"></param>
    /// <param name="useLow"></param>
    /// <param name="useUpp"></param>
    /// <param name="useSpe"></param>
    /// <param name="custom"></param>
    /// <returns></returns>
    public static string GetRandom(
        int length = 4,
        bool useNum = true,
        bool useLow = false,
        bool useUpp = true,
        bool useSpe = false,
        string custom = ""
    )
    {

        var sb = new StringBuilder(custom);
        if (useNum)
        {
            sb.Append("0123456789");
        }
        if (useLow)
        {
            sb.Append("abcdefghijklmnopqrstuvwxyz");
        }
        if (useUpp)
        {
            sb.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        }
        if (useSpe)
        {
            sb.Append("!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~");
        }

        ReadOnlySpan<char> strSpan = sb.ToString().AsSpan();
        var resultBuilder = new StringBuilder(length);

        for (var i = 0; i < length; i++)
        {
            var position = RandomNumberGenerator.GetInt32(strSpan.Length);
            resultBuilder.Append(strSpan[position]);
        }
        return resultBuilder.ToString();
    }

    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="text">源文</param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string AesEncrypt(string text, string key)
    {
        byte[] encrypted;
        var bytes = Encoding.UTF8.GetBytes(text);
        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(Md5Hash(key));
            aesAlg.IV = aesAlg.Key[..16];
            ICryptoTransform encryptor = aesAlg.CreateEncryptor();
            using MemoryStream memoryStream = new();
            using var csEncrypt = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            csEncrypt.Write(bytes, 0, bytes.Length);
            csEncrypt.FlushFinalBlock();
            encrypted = memoryStream.ToArray();
        }
        return Convert.ToBase64String(encrypted);
    }

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="cipherText"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string AesDecrypt(string cipherText, string key)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
        {
            return string.Empty;
        }
        string? plaintext = null;
        using (var aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(Md5Hash(key));
            aesAlg.IV = aesAlg.Key[..16];
            ICryptoTransform decryptor = aesAlg.CreateDecryptor();
            using MemoryStream msDecrypt = new(Convert.FromBase64String(cipherText));
            using CryptoStream csDecrypt = new(msDecrypt, decryptor, CryptoStreamMode.Read);
            using StreamReader srDecrypt = new(csDecrypt, Encoding.UTF8);
            plaintext = srDecrypt.ReadToEnd();
        }
        return plaintext;
    }

    /// <summary>
    /// json对象加密
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string JsonEncrypt(object data)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(data, JsonSerializerOptions);

        if (bytes != null)
        {
            bytes = bytes.Select(b => b == byte.MaxValue ? byte.MinValue : (byte)(b + 1)).ToArray();
            Array.Reverse(bytes);
            return Convert.ToBase64String(bytes);
        }
        return string.Empty;
    }

    /// <summary>
    /// json对象解密
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static T? JsonDecrypt<T>(string value)
        where T : class
    {
        var bytes = Convert.FromBase64String(value);
        if (bytes != null)
        {
            Array.Reverse(bytes);
            bytes = bytes.Select(b => b == byte.MinValue ? byte.MaxValue : (byte)(b - 1)).ToArray();
            var jsonString = Encoding.UTF8.GetString(bytes);

            return JsonSerializer.Deserialize<T>(jsonString, JsonSerializerOptions);
        }
        return null;
    }
}

/// <summary>
/// hash type
/// </summary>
public enum HashType
{
    MD5,
    SHA256,
    SHA512,
}
