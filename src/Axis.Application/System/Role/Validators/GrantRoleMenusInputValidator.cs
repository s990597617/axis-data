using Axis.Application.System.Role.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.Role.Validators;

/// <summary>
/// <see cref="GrantRoleMenusInput"/> 的入参格式校验。
/// </summary>
/// <remarks>
/// 仅校验 Id 与集合本身；菜单是否存在由 <see cref="IRoleAppService.GrantMenusAsync"/> 校验。
/// </remarks>
public sealed class GrantRoleMenusInputValidator : Validator<GrantRoleMenusInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public GrantRoleMenusInputValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("角色 Id 无效");

        RuleFor(x => x.MenuIds)
            .NotNull().WithMessage("菜单授权列表不能为 null");
    }
}
