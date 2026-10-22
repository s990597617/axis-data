using Axis.Application.System.Job.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.Job.Validators;

/// <summary>
/// 保存定时任务校验。
/// </summary>
public sealed class SaveJobInputValidator : Validator<SaveJobInput>
{
    public SaveJobInputValidator()
    {
        RuleFor(x => x.JobCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Cron).NotEmpty().MaximumLength(128);
        RuleFor(x => x.GroupName).MaximumLength(64).When(x => x.GroupName is not null);
        RuleFor(x => x.Params).MaximumLength(2000).When(x => x.Params is not null);
        RuleFor(x => x.Remark).MaximumLength(256).When(x => x.Remark is not null);
        RuleFor(x => x.OrderNo).InclusiveBetween(0, 9999);
    }
}
