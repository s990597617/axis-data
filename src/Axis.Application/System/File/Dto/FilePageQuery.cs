namespace Axis.Application.System.File.Dto;

/// <summary>
/// 文件管理分页查询条件。
/// </summary>
public sealed class FilePageQuery
{
    /// <summary>
    /// 页码，从 1 开始。
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页条数。
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 关键字；匹配原始文件名、FileId、相对路径；可为空。
    /// </summary>
    public string? Keyword { get; set; }

    /// <summary>
    /// 业务模块标识过滤（如 avatar、common）；可为空表示不过滤。
    /// </summary>
    public string? Module { get; set; }
}
