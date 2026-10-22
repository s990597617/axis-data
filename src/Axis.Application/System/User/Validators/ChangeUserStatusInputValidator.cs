using Axis.Application.System.User.Dto;
using Axis.Domain.Enums;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.User.Validators;

/// <summary>
/// <see cref="ChangeUserStatusInput"/> 的入参格式校验。
/// </summary>
public sealed class ChangeUserStatusInputValidator : Validator<ChangeUserStatusInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public ChangeUserStatusInputValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("用户 Id 无效");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("用户状态无效")
            .Must(status => status is StatusEnum.Enable or StatusEnum.Disable)
            .WithMessage("用户状态仅支持启用或禁用");
    }
}
