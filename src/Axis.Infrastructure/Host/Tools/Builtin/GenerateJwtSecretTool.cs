using System.Security.Cryptography;

namespace Axis.Infrastructure.Host.Tools.Builtin;

/// <summary>
/// 生成 JWT Secret 配置密钥
/// </summary>
[HostTool("gen-jwt-secret", "生成 JWT Secret 配置密钥")]
public class GenerateJwtSecretTool: IHostTool
{
    public void Execute(string[] args)
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        var secret = Convert.ToBase64String(bytes);

        Console.WriteLine("===== JWT Secret =====");
        Console.WriteLine(secret);
        Console.WriteLine("======================");
    }
}