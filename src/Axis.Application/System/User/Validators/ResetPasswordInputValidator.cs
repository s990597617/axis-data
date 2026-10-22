using Axis.Application.System.User.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.User.Validators;

/// <summary>
/// <see cref="ResetPasswordInput"/> 的入参格式校验。
/// </summary>
public sealed class ResetPasswordInputValidator : Validator<ResetPasswordInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public ResetPasswordInputValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("用户 Id 无效");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("新密码不能为空")
            .MinimumLength(6).WithMessage("新密码不能少于 6 个字符")
            .MaximumLength(64).WithMessage("新密码不能超过 64 个字符");
    }
}
