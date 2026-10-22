using System.Text.Json;
using Axis.Domain.Data.System.Repository;
using Axis.Domain.System.Seeds;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Jobs;

namespace Axis.Host.Api.Jobs;

/// <summary>
/// 定时清理无效刷新令牌，防止 <c>sys_refresh_token</c> 无限膨胀。
/// </summary>
/// <remarks>
/// 删除条件：过期时间 / 吊销时间 / 消费时间早于「当前时间 - retainDays」。
/// 参数 JSON 示例：<c>{"retainDays":7}</c>；缺省保留 7 天。
/// </remarks>
[AxisJob(
    "清理刷新令牌",
    "0 0 3 * * ?",
    GroupName = "system",
    Builtin = true,
    Enabled = true,
    OrderNo = 10,
    DefaultParams = """{"retainDays":7}""",
    Remark = "每天 03:00 清理过期/已吊销/已消费超过保留天数的刷新令牌")]
public sealed class CleanupRefreshTokenJob(
    ISysRefreshTokenRepository refreshTokenRepo,
    ILogger<CleanupRefreshTokenJob> logger) : IAxisJob, IAutoInject
{
    private const int DefaultRetainDays = 7;
    private const int MinRetainDays = 1;
    private const int MaxRetainDays = 365;

    /// <summary>
    /// 任务编码，与种子 <see cref="SysJobSeed.CleanupRefreshTokenJobCode"/> 对齐。
    /// </summary>
    public string Code => SysJobSeed.CleanupRefreshTokenJobCode;

    /// <summary>
    /// 按保留天数清理无效刷新令牌。
    /// </summary>
    /// <param name="context">执行上下文（可读 Params）。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task ExecuteAsync(AxisJobContext context, CancellationToken ct)
    {
        var retainDays = ResolveRetainDays(context.Params);
        var cutoff = DateTime.Now.AddDays(-retainDays);
        var deleted = await refreshTokenRepo.DeleteObsoleteAsync(cutoff, ct);

        logger.LogInformation(
            "清理刷新令牌完成：RetainDays={RetainDays}, Cutoff={Cutoff:yyyy-MM-dd HH:mm:ss}, Deleted={Deleted}",
            retainDays,
            cutoff,
            deleted);
    }

    private static int ResolveRetainDays(string? rawParams)
    {
        if (string.IsNullOrWhiteSpace(rawParams))
            return DefaultRetainDays;

        try
        {
            using var doc = JsonDocument.Parse(rawParams);
            if (!doc.RootElement.TryGetProperty("retainDays", out var node))
                return DefaultRetainDays;

            var days = node.ValueKind == JsonValueKind.Number
                ? node.GetInt32()
                : int.TryParse(node.GetString(), out var parsed) ? parsed : DefaultRetainDays;

            return Math.Clamp(days, MinRetainDays, MaxRetainDays);
        }
        catch (JsonException)
        {
            return DefaultRetainDays;
        }
    }
}
