namespace Axis.Application.System.Config.Dto;

/// <summary>
/// 仅携带参数配置主键的路径参数。
/// </summary>
public sealed class ConfigIdRequest
{
    /// <summary>
    /// 参数主键 Id。
    /// </summary>
    public long Id { get; set; }
}
