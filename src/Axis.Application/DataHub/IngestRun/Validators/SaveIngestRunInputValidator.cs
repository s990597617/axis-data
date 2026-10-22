using Axis.Application.DataHub.IngestRun.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.DataHub.IngestRun.Validators;

/// <summary>
/// <see cref="SaveIngestRunInput"/> 请求体格式校验。
/// </summary>
public sealed class SaveIngestRunInputValidator : Validator<SaveIngestRunInput>
{
    public SaveIngestRunInputValidator()
    {
        RuleFor(x => x.JobId)
            .GreaterThan(0).WithMessage("请选择采集任务");
        RuleFor(x => x.Name)
            .MaximumLength(128).WithMessage("名称不能超过 128 个字符")
            .When(x => x.Name is not null);
        RuleFor(x => x.Code)
            .MaximumLength(64).WithMessage("编码不能超过 64 个字符")
            .When(x => x.Code is not null);
        RuleFor(x => x.ErrorMessage)
            .MaximumLength(1024).WithMessage("错误信息不能超过 1024 个字符")
            .When(x => x.ErrorMessage is not null);
        RuleFor(x => x.Watermark)
            .MaximumLength(512).WithMessage("水位线不能超过 512 个字符")
            .When(x => x.Watermark is not null);
        RuleFor(x => x.Remark)
            .MaximumLength(256).WithMessage("备注不能超过 256 个字符")
            .When(x => x.Remark is not null);
    }
}
