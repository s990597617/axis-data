namespace Axis.Application.Mdm.Process.Dto;

/// <summary>
/// 启用工序的下拉选项。
/// </summary>
public sealed class ProcessOptionDto
{
    /// <summary>
    /// 工序主键。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 工序编码。
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 工序名称。
    /// </summary>
    public string Name { get; set; } = null!;
}
