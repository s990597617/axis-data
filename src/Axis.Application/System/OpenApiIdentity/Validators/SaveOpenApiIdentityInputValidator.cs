using Axis.Application.System.OpenApiIdentity.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.OpenApiIdentity.Validators;

/// <summary>
/// <see cref="SaveOpenApiIdentityInput"/> 格式校验。
/// </summary>
public sealed class SaveOpenApiIdentityInputValidator : Validator<SaveOpenApiIdentityInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public SaveOpenApiIdentityInputValidator()
    {
        RuleFor(x => x.AccessKey)
            .NotEmpty().WithMessage("身份标识不能为空")
            .MaximumLength(64).WithMessage("身份标识不能超过 64 个字符")
            .Matches(@"^[A-Za-z0-9_-]+$").WithMessage("身份标识仅允许字母、数字、下划线与中划线");

        RuleFor(x => x.AccessSecret)
            .MaximumLength(128).WithMessage("密钥不能超过 128 个字符")
            .When(x => !string.IsNullOrWhiteSpace(x.AccessSecret));

        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("请选择绑定用户");

        RuleFor(x => x.Remark)
            .MaximumLength(256).WithMessage("备注不能超过 256 个字符")
            .When(x => x.Remark is not null);
    }
}
