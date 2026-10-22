using Axis.Application.System.Profile.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.Profile.Validators;

/// <summary>
/// <see cref="ChangePasswordInput"/> 的入参格式校验。
/// </summary>
/// <remarks>
/// 仅校验密文非空与长度；明文长度、两次确认一致性、新旧是否相同在服务端解密后处理。
/// </remarks>
public sealed class ChangePasswordInputValidator : Validator<ChangePasswordInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public ChangePasswordInputValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage("当前密码不能为空")
            .MaximumLength(1024).WithMessage("当前密码密文不能超过 1024 个字符");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("新密码不能为空")
            .MaximumLength(1024).WithMessage("新密码密文不能超过 1024 个字符");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("确认密码不能为空")
            .MaximumLength(1024).WithMessage("确认密码密文不能超过 1024 个字符");
    }
}
