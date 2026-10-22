using Axis.Application.DataHub.Dataset.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.DataHub.Dataset.Validators;

public sealed class SaveDatasetInputValidator : Validator<SaveDatasetInput>
{
    public SaveDatasetInputValidator()
    {
        RuleFor(x => x.SourceId).GreaterThan(0).WithMessage("请选择数据源");
        RuleFor(x => x.Name).NotEmpty().WithMessage("名称不能为空").MaximumLength(64);
        RuleFor(x => x.Code).NotEmpty().WithMessage("编码不能为空").MaximumLength(64);
        RuleFor(x => x.ObjectName).MaximumLength(256).When(x => x.ObjectName is not null);
        RuleFor(x => x.PrimaryKeyFields).MaximumLength(256).When(x => x.PrimaryKeyFields is not null);
        RuleFor(x => x.IncrementalField).MaximumLength(128).When(x => x.IncrementalField is not null);
        RuleFor(x => x.Remark).MaximumLength(256).When(x => x.Remark is not null);
    }
}
