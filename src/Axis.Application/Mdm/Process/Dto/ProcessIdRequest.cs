namespace Axis.Application.Mdm.Process.Dto;

/// <summary>
/// 仅携带工序主键的路径参数。
/// </summary>
public sealed class ProcessIdRequest
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }
}
