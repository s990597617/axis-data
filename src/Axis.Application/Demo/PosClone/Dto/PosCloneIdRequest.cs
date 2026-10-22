namespace Axis.Application.Demo.PosClone.Dto;

/// <summary>
/// 仅携带岗位副本主键的路径参数。
/// </summary>
public sealed class PosCloneIdRequest
{
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }
}
