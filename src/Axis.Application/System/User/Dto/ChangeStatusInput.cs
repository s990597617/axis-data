using Axis.Domain.Enums;

namespace Axis.Application.System.User.Dto;

/// <summary>
/// 修改用户启用状态的输入数据。
/// </summary>
public sealed class ChangeStatusInput
{
    /// <summary>
    /// 用户主键 Id。
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 目标状态（启用 / 禁用）。
    /// </summary>
    public StatusEnum Status { get; set; }
}
