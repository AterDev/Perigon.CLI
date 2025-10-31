namespace Ater.AspNetCore.Utils;

/// <summary>
/// RSA帮助类
/// </summary>
public class RSAHelper
{
    /// <summary>
    /// 生成公钥和私钥的RSA密钥对。
    /// </summary>
    /// <returns>包含公钥和私钥的元组。</returns>
    public static (string publicKey, string privateKey) GenerateKeys()
    {
        using var rsa = RSA.Create();
        return (
            Convert.ToBase64String(rsa.ExportRSAPublicKey()),
            Convert.ToBase64String(rsa.ExportRSAPrivateKey())
        );
    }

    /// <summary>
    /// 使用提供的公钥加密指定的明文。
    /// </summary>
    /// <param name="publicKey">公钥。</param>
    /// <param name="plainText">要加密的明文。</param>
    /// <returns>加密的数据。</returns>
    public static byte[] Encrypt(string publicKey, string plainText)
    {
        using var rsa = RSA.Create();
        rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
        return rsa.Encrypt(Encoding.UTF8.GetBytes(plainText), RSAEncryptionPadding.OaepSHA256);
    }

    /// <summary>
    /// 使用提供的私钥解密指定的加密数据。
    /// </summary>
    /// <param name="privateKey">私钥。</param>
    /// <param name="encryptedData">要解密的数据。</param>
    /// <returns>解密的字符串。</returns>
    public static string Decrypt(string privateKey, byte[] encryptedData)
    {
        using var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
        var decryptedBytes = rsa.Decrypt(encryptedData, RSAEncryptionPadding.OaepSHA256);
        return Encoding.UTF8.GetString(decryptedBytes);
    }

    /// <summary>
    /// 使用提供的私钥签名指定的数据。
    /// </summary>
    /// <param name="privateKey">私钥。</param>
    /// <param name="data">要签名的数据。</param>
    /// <returns>签名。</returns>
    public static byte[] SignData(string privateKey, ReadOnlySpan<byte> data)
    {
        using var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
        return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    /// <summary>
    /// 使用提供的公钥验证指定的数据和签名。
    /// </summary>
    /// <param name="publicKey">公钥。</param>
    /// <param name="data">要验证的数据。</param>
    /// <param name="signature">要验证的签名。</param>
    /// <returns>如果数据和签名有效，则为true；否则为false。</returns>
    public static bool VerifyData(
        string publicKey,
        ReadOnlySpan<byte> data,
        ReadOnlySpan<byte> signature
    )
    {
        using var rsa = RSA.Create();
        rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey), out _);
        return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }
}
