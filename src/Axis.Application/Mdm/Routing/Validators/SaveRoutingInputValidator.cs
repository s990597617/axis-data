using Axis.Application.Mdm.Routing.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.Mdm.Routing.Validators;

/// <summary>
/// <see cref="SaveRoutingInput"/> 的入参格式校验。
/// </summary>
/// <remarks>
/// 编码唯一性与工序存在性由应用服务校验。
/// </remarks>
public sealed class SaveRoutingInputValidator : Validator<SaveRoutingInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public SaveRoutingInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("路线名称不能为空")
            .MaximumLength(64).WithMessage("路线名称不能超过 64 个字符");

        RuleFor(x => x.Code)
            .MaximumLength(64).WithMessage("路线编码不能超过 64 个字符")
            .When(x => x.Code is not null);

        RuleFor(x => x.Remark)
            .MaximumLength(256).WithMessage("备注不能超过 256 个字符")
            .When(x => x.Remark is not null);

        RuleFor(x => x.Steps)
            .NotEmpty().WithMessage("至少添加一道工序")
            .Must(HaveUniqueSeq).WithMessage("工序序号不能重复")
            .Must(HaveReportPoint).WithMessage("至少需要一个报工点");

        RuleForEach(x => x.Steps).ChildRules(step =>
        {
            step.RuleFor(item => item.Seq)
                .GreaterThan(0).WithMessage("工序序号必须大于 0");
            step.RuleFor(item => item.ProcessId)
                .GreaterThan(0).WithMessage("请选择工序");
            step.RuleFor(item => item.StandardMinutes)
                .GreaterThanOrEqualTo(0).WithMessage("标准工时不能为负数");
            step.RuleFor(item => item.Remark)
                .MaximumLength(256).WithMessage("行备注不能超过 256 个字符")
                .When(item => item.Remark is not null);
        });
    }

    private static bool HaveUniqueSeq(List<SaveRoutingStepInput> steps)
        => steps.Select(item => item.Seq).Distinct().Count() == steps.Count;

    private static bool HaveReportPoint(List<SaveRoutingStepInput> steps)
        => steps.Any(item => item.IsReportPoint);
}
