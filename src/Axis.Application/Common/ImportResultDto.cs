namespace Axis.Application.Common;

/// <summary>
/// 批量导入结果。
/// </summary>
public sealed class ImportResultDto
{
    /// <summary>
    /// 成功写入的行数。
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败行数。
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// 失败明细（行号含表头，首行数据一般为 2）。
    /// </summary>
    public List<ImportFailureDto> Failures { get; set; } = [];
}
