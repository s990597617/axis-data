namespace Axis.Application.DataHub.Source.Dto;

/// <summary>
/// 连通性测试结果。
/// </summary>
public sealed class TestSourceConnectionResultDto
{
    /// <summary>
    /// 是否连通成功。
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 结果说明。
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 耗时（毫秒）。
    /// </summary>
    public int ElapsedMs { get; set; }
}