using Axis.Infrastructure.Security;

namespace Axis.Infrastructure.Host.Tools.Builtin;

/// <summary>
/// SM2 密钥对生成工具。
/// </summary>
[HostTool("gen-sm2", "生成一次性 SM2 公钥和私钥，用于初始化 System.PublicKey / System.PrivateKey 配置。")]
public sealed class GenerateSm2KeyTool : IHostTool
{
    public void Execute(string[] args)
    {
        var (publicKey, privateKey) = SmTools.GenerateKeyPair();

        Console.WriteLine("===== SM2 KeyPair =====");
        Console.WriteLine($"PublicKey  : {publicKey}");
        Console.WriteLine($"PrivateKey : {privateKey}");
        Console.WriteLine("=======================");
        Console.WriteLine("将 PublicKey 配置到前端 axis-web/.env 的 VITE_SM2_PUBLIC_KEY；");
        Console.WriteLine("将 PublicKey + PrivateKey 配置到 appsettings.json 的 System 节。");
    }
}