namespace Axis.Application.Mdm.Process.Dto;

/// <summary>
/// 工序下一编码预览；不占用流水。
/// </summary>
public sealed class ProcessNextCodeDto
{
    /// <summary>
    /// 预览编码。
    /// </summary>
    public string Code { get; set; } = null!;
}
