using Axis.Application.System.Profile.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.Profile.Validators;

/// <summary>
/// <see cref="UpdateProfileInput"/> 的入参格式校验。
/// </summary>
public sealed class UpdateProfileInputValidator : Validator<UpdateProfileInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public UpdateProfileInputValidator()
    {
        RuleFor(x => x.RealName)
            .NotEmpty().WithMessage("真实姓名不能为空")
            .MaximumLength(32).WithMessage("真实姓名不能超过 32 个字符");

        RuleFor(x => x.NickName)
            .MaximumLength(32).WithMessage("昵称不能超过 32 个字符")
            .When(x => !string.IsNullOrWhiteSpace(x.NickName));

        RuleFor(x => x.Avatar)
            .MaximumLength(512).WithMessage("头像地址不能超过 512 个字符")
            .When(x => !string.IsNullOrWhiteSpace(x.Avatar));

        RuleFor(x => x.Phone)
            .MaximumLength(16).WithMessage("手机号不能超过 16 个字符")
            .When(x => !string.IsNullOrWhiteSpace(x.Phone));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("邮箱格式不正确")
            .MaximumLength(64).WithMessage("邮箱不能超过 64 个字符")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Remark)
            .MaximumLength(256).WithMessage("备注不能超过 256 个字符")
            .When(x => !string.IsNullOrWhiteSpace(x.Remark));
    }
}
