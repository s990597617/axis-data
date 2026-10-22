using Axis.Application.DataHub.IngestJob.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.DataHub.IngestJob.Validators;

public sealed class SaveIngestJobInputValidator : Validator<SaveIngestJobInput>
{
    public SaveIngestJobInputValidator()
    {
        RuleFor(x => x.DatasetId).GreaterThan(0).WithMessage("请选择数据集");
        RuleFor(x => x.Name).NotEmpty().WithMessage("名称不能为空").MaximumLength(64);
        RuleFor(x => x.Code).NotEmpty().WithMessage("编码不能为空").MaximumLength(64);
        RuleFor(x => x.CronExpression).MaximumLength(64).When(x => x.CronExpression is not null);
        RuleFor(x => x.BatchSize).GreaterThan(0).WithMessage("批大小必须大于 0");
        RuleFor(x => x.Remark).MaximumLength(256).When(x => x.Remark is not null);
    }
}
