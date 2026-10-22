using Axis.Application.DataHub.Dataset.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.DataHub.Dataset.Validators;

/// <summary>
/// <see cref="SaveDatasetInput"/> 的入参格式校验。
/// </summary>
public sealed class SaveDatasetInputValidator : Validator<SaveDatasetInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public SaveDatasetInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name不能为空")
            .MaximumLength(64).WithMessage("Name不能超过 64 个字符");
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code不能为空")
            .MaximumLength(64).WithMessage("Code不能超过 64 个字符");
        RuleFor(x => x.ObjectName)
            .MaximumLength(256).WithMessage("ObjectName不能超过 256 个字符")
            .When(x => x.ObjectName is not null);
        RuleFor(x => x.PrimaryKeyFields)
            .MaximumLength(256).WithMessage("PrimaryKeyFields不能超过 256 个字符")
            .When(x => x.PrimaryKeyFields is not null);
        RuleFor(x => x.IncrementalField)
            .MaximumLength(128).WithMessage("IncrementalField不能超过 128 个字符")
            .When(x => x.IncrementalField is not null);
        RuleFor(x => x.Remark)
            .MaximumLength(256).WithMessage("Remark不能超过 256 个字符")
            .When(x => x.Remark is not null);
    }
}
