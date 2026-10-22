using System.Text;
using Org.BouncyCastle.Asn1.GM;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace Axis.Infrastructure.Security;

/// <summary>
/// 国密 SM2 工具类（基于 BouncyCastle）。
/// </summary>
/// <remarks>
/// 与业界约定保持一致：曲线为 <c>sm2p256v1</c>；密文默认结构 C1C3C2，编码为 HEX。
/// 主要用于登录等场景下前端公钥加密、服务端私钥解密传输敏感字段（如密码）。
/// </remarks>
public static class SmTools
{
    private static readonly X9ECParameters Curve = GMNamedCurves.GetByName("sm2p256v1");

    private static readonly ECDomainParameters DomainParameters =
        new(Curve.Curve, Curve.G, Curve.N, Curve.H);

    /// <summary>
    /// 使用 SM2 公钥加密明文。
    /// </summary>
    /// <param name="publicKeyHex">HEX 公钥（未压缩点，通常 130 位含 <c>04</c> 前缀，或省略前缀的 128 位）。</param>
    /// <param name="plain">明文字符串（UTF-8 编码后加密）。</param>
    /// <param name="mode">密文结构；默认 <see cref="SM2Engine.Mode.C1C3C2"/>（国密标准），可选 C1C2C3 以兼容旧实现。</param>
    /// <returns>HEX 编码的密文字符串。</returns>
    /// <exception cref="ArgumentException"><paramref name="publicKeyHex"/> 为空。</exception>
    /// <exception cref="ArgumentNullException"><paramref name="plain"/> 为 <c>null</c>。</exception>
    public static string Sm2Encrypt(string publicKeyHex, string plain, SM2Engine.Mode mode = SM2Engine.Mode.C1C3C2)
    {
        if (string.IsNullOrEmpty(publicKeyHex)) throw new ArgumentException("公钥不能为空", nameof(publicKeyHex));
        if (plain == null) throw new ArgumentNullException(nameof(plain));

        var pubKey = DecodePublicKey(publicKeyHex);
        var engine = new SM2Engine(mode);
        engine.Init(true, new ParametersWithRandom(pubKey, new SecureRandom()));

        var data = Encoding.UTF8.GetBytes(plain);
        var cipher = engine.ProcessBlock(data, 0, data.Length);
        return Hex.ToHexString(cipher);
    }

    /// <summary>
    /// 使用 SM2 私钥解密密文。
    /// </summary>
    /// <param name="privateKeyHex">HEX 编码的私钥标量。</param>
    /// <param name="cipherHex">HEX 密文；默认按 C1C3C2 解析，可通过 <paramref name="mode"/> 切换。</param>
    /// <param name="mode">密文结构；默认 C1C3C2。</param>
    /// <returns>UTF-8 解码后的明文字符串。</returns>
    /// <exception cref="ArgumentException"><paramref name="privateKeyHex"/> 或 <paramref name="cipherHex"/> 为空。</exception>
    public static string Sm2Decrypt(string privateKeyHex, string cipherHex, SM2Engine.Mode mode = SM2Engine.Mode.C1C3C2)
    {
        if (string.IsNullOrEmpty(privateKeyHex)) throw new ArgumentException("私钥不能为空", nameof(privateKeyHex));
        if (string.IsNullOrEmpty(cipherHex)) throw new ArgumentException("密文不能为空", nameof(cipherHex));

        var priKey = new ECPrivateKeyParameters(new BigInteger(privateKeyHex, 16), DomainParameters);
        var engine = new SM2Engine(mode);
        engine.Init(false, priKey);

        var cipher = Hex.Decode(NormalizeCipher(cipherHex));
        var plain = engine.ProcessBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(plain);
    }

    /// <summary>
    /// 生成一对 SM2 密钥（HEX 编码）。
    /// </summary>
    /// <returns>
    /// <c>PublicKey</c>：含 <c>04</c> 前缀的未压缩公钥点 HEX；
    /// <c>PrivateKey</c>：64 位 HEX 私钥。
    /// </returns>
    /// <remarks>
    /// 用于框架初始化或开发联调时一次性产出公私钥对，建议写入配置后重启使用；
    /// 切勿将生产环境密钥提交到代码仓库。
    /// </remarks>
    public static (string PublicKey, string PrivateKey) GenerateKeyPair()
    {
        var generator = new ECKeyPairGenerator("EC");
        var domain = DomainParameters;
        generator.Init(new ECKeyGenerationParameters(domain, new SecureRandom()));
        var keyPair = generator.GenerateKeyPair();

        var priv = (ECPrivateKeyParameters)keyPair.Private;
        var pub = (ECPublicKeyParameters)keyPair.Public;

        var privateHex = priv.D.ToString(16).PadLeft(64, '0').ToUpperInvariant();
        var publicHex = Hex.ToHexString(pub.Q.GetEncoded(false)).ToUpperInvariant();
        return (publicHex, privateHex);
    }

    /// <summary>
    /// 解析未压缩 HEX 公钥：长度为 128 时自动补全 <c>04</c> 前缀。
    /// </summary>
    /// <param name="publicKeyHex">HEX 公钥字符串。</param>
    /// <returns>可供 SM2 引擎使用的公钥参数。</returns>
    private static ECPublicKeyParameters DecodePublicKey(string publicKeyHex)
    {
        var hex = publicKeyHex.Trim();
        if (hex.Length == 128) hex = "04" + hex;
        var pubBytes = Hex.Decode(hex);
        ECPoint point = Curve.Curve.DecodePoint(pubBytes);
        return new ECPublicKeyParameters(point, DomainParameters);
    }

    /// <summary>
    /// 规范化 HEX 密文：部分前端未携带 <c>04</c> 起始标记时自动补齐，确保 BouncyCastle 可解析。
    /// </summary>
    /// <param name="cipherHex">原始 HEX 密文。</param>
    /// <returns>必要时补前缀后的 HEX 密文。</returns>
    private static string NormalizeCipher(string cipherHex)
    {
        var hex = cipherHex.Trim();
        // 未压缩点以 04 开头；长度 >= 128（仅 C1 部分 64 字节 = 128 HEX 字符），缺失时补上
        if (hex.Length >= 128 && !hex.StartsWith("04", StringComparison.OrdinalIgnoreCase))
        {
            hex = "04" + hex;
        }
        return hex;
    }
}
