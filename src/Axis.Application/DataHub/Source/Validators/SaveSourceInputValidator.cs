using Axis.Application.DataHub.Source.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.DataHub.Source.Validators;

/// <summary>
/// <see cref="SaveSourceInput"/> 的入参格式校验。
/// </summary>
public sealed class SaveSourceInputValidator : Validator<SaveSourceInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public SaveSourceInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("名称不能为空")
            .MaximumLength(64).WithMessage("名称不能超过 64 个字符");
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("编码不能为空")
            .MaximumLength(64).WithMessage("编码不能超过 64 个字符");
        RuleFor(x => x.SecretRef)
            .MaximumLength(128).WithMessage("密钥引用不能超过 128 个字符")
            .When(x => x.SecretRef is not null);
        RuleFor(x => x.ConnectionString)
            .MaximumLength(1024).WithMessage("连接串不能超过 1024 个字符")
            .When(x => x.ConnectionString is not null);
        RuleFor(x => x.Remark)
            .MaximumLength(256).WithMessage("备注不能超过 256 个字符")
            .When(x => x.Remark is not null);
    }
}