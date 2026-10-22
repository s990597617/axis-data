namespace Axis.Application.System.File.Dto;

/// <summary>
/// 文件业务模块统计项。
/// </summary>
public sealed class FileModuleStatDto
{
    /// <summary>
    /// 业务模块标识；空字符串表示未分类。
    /// </summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// 该模块下未删除文件数量。
    /// </summary>
    public int Count { get; set; }
}
