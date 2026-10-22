using Axis.Application.System.CodeRule.Dto;
using Axis.Domain.Enums;
using Axis.Domain.System;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.CodeRule.Validators;

/// <summary>
/// <see cref="SaveCodeRuleInput"/> 的入参格式校验。
/// </summary>
/// <remarks>
/// 仅做格式与必填；编码唯一性与取号由应用服务负责。
/// </remarks>
public sealed class SaveCodeRuleInputValidator : Validator<SaveCodeRuleInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public SaveCodeRuleInputValidator()
    {
        RuleFor(item => item.Name)
            .NotEmpty().WithMessage("规则名称不能为空")
            .MaximumLength(64).WithMessage("规则名称不能超过 64 个字符");

        RuleFor(item => item.Code)
            .NotEmpty().WithMessage("规则编码不能为空")
            .MaximumLength(64).WithMessage("规则编码不能超过 64 个字符")
            .Matches(@"^[A-Za-z][A-Za-z0-9_-]*$").WithMessage("规则编码须以字母开头，仅含字母数字下划线与连字符");

        RuleFor(item => item.Prefix)
            .MaximumLength(32).WithMessage("前缀不能超过 32 个字符")
            .When(item => item.Prefix is not null);

        RuleFor(item => item.DateFormat)
            .MaximumLength(32).WithMessage("日期格式不能超过 32 个字符")
            .When(item => item.DateFormat is not null);

        RuleFor(item => item.SerialLength)
            .InclusiveBetween(1, 16).WithMessage("流水长度须在 1～16 之间");

        RuleFor(item => item.ResetCycle)
            .IsInEnum().WithMessage("重置周期无效")
            .Must(cycle => cycle != CodeResetCycleEnum.Unknown).WithMessage("请选择重置周期");

        RuleFor(item => item.Template)
            .NotEmpty().WithMessage("拼接模板不能为空")
            .MaximumLength(128).WithMessage("拼接模板不能超过 128 个字符");

        RuleFor(item => item.CheckAlgo)
            .IsInEnum().WithMessage("校验位算法无效")
            .Must(algo => algo != CodeCheckAlgoEnum.Unknown).WithMessage("请选择校验位算法");

        RuleFor(item => item.SourceType)
            .IsInEnum().WithMessage("来源类型无效")
            .Must(type => type != CodeSourceTypeEnum.Unknown).WithMessage("请选择来源类型");

        RuleFor(item => item.ReverseRegex)
            .MaximumLength(512).WithMessage("反向正则不能超过 512 个字符")
            .When(item => item.ReverseRegex is not null);

        RuleFor(item => item.Remark)
            .MaximumLength(256).WithMessage("备注不能超过 256 个字符")
            .When(item => item.Remark is not null);

        RuleFor(item => item.DateFormat)
            .NotEmpty().WithMessage("模板包含 {Date} 时必须填写日期格式")
            .When(item => item.Template is not null
                && item.Template.Contains("{" + CodePlaceholderNames.Date + "}", StringComparison.Ordinal));

        RuleFor(item => item.CheckAlgo)
            .Must(algo => algo != CodeCheckAlgoEnum.None)
            .WithMessage("模板包含 {Check} 时必须选择校验位算法")
            .When(item => item.Template is not null
                && item.Template.Contains("{" + CodePlaceholderNames.Check + "}", StringComparison.Ordinal));
    }
}
