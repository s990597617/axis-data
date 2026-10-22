using Axis.Application.DataHub.IngestRun.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.DataHub.IngestRun.Validators;

/// <summary>
/// <see cref="SaveIngestRunInput"/> 的入参格式校验。
/// </summary>
public sealed class SaveIngestRunInputValidator : Validator<SaveIngestRunInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public SaveIngestRunInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name不能为空")
            .MaximumLength(128).WithMessage("Name不能超过 128 个字符");
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code不能为空")
            .MaximumLength(64).WithMessage("Code不能超过 64 个字符");
        RuleFor(x => x.ErrorMessage)
            .MaximumLength(1024).WithMessage("ErrorMessage不能超过 1024 个字符")
            .When(x => x.ErrorMessage is not null);
        RuleFor(x => x.Watermark)
            .MaximumLength(512).WithMessage("Watermark不能超过 512 个字符")
            .When(x => x.Watermark is not null);
        RuleFor(x => x.Remark)
            .MaximumLength(256).WithMessage("Remark不能超过 256 个字符")
            .When(x => x.Remark is not null);
    }
}
