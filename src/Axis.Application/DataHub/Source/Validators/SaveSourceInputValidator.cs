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
            .NotEmpty().WithMessage("Name不能为空")
            .MaximumLength(64).WithMessage("Name不能超过 64 个字符");
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code不能为空")
            .MaximumLength(64).WithMessage("Code不能超过 64 个字符");
        RuleFor(x => x.SecretRef)
            .MaximumLength(128).WithMessage("SecretRef不能超过 128 个字符")
            .When(x => x.SecretRef is not null);
        RuleFor(x => x.Remark)
            .MaximumLength(256).WithMessage("Remark不能超过 256 个字符")
            .When(x => x.Remark is not null);
    }
}
