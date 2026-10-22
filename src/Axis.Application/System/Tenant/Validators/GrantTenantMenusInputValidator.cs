using Axis.Application.System.Tenant.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.Tenant.Validators;

/// <summary>
/// <see cref="GrantTenantMenusInput"/> 的入参格式校验。
/// </summary>
public sealed class GrantTenantMenusInputValidator : Validator<GrantTenantMenusInput>
{
    public GrantTenantMenusInputValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("租户 Id 无效");

        RuleFor(x => x.MenuIds)
            .NotNull().WithMessage("菜单授权列表不能为 null");
    }
}
