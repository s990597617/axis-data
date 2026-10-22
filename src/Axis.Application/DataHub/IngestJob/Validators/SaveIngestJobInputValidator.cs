using Axis.Application.DataHub.IngestJob.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.DataHub.IngestJob.Validators;

/// <summary>
/// <see cref="SaveIngestJobInput"/> 的入参格式校验。
/// </summary>
public sealed class SaveIngestJobInputValidator : Validator<SaveIngestJobInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public SaveIngestJobInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name不能为空")
            .MaximumLength(64).WithMessage("Name不能超过 64 个字符");
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code不能为空")
            .MaximumLength(64).WithMessage("Code不能超过 64 个字符");
        RuleFor(x => x.CronExpression)
            .MaximumLength(64).WithMessage("CronExpression不能超过 64 个字符")
            .When(x => x.CronExpression is not null);
        RuleFor(x => x.Remark)
            .MaximumLength(256).WithMessage("Remark不能超过 256 个字符")
            .When(x => x.Remark is not null);
    }
}
