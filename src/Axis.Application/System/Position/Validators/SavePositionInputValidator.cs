using Axis.Application.System.Position.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.Position.Validators;

/// <summary>
/// <see cref="SavePositionInput"/> 的入参格式校验。
/// </summary>
public sealed class SavePositionInputValidator : Validator<SavePositionInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public SavePositionInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("岗位名称不能为空")
            .MaximumLength(64).WithMessage("岗位名称不能超过 64 个字符");

        RuleFor(x => x.Code)
            .MaximumLength(64).WithMessage("岗位编码不能超过 64 个字符")
            .When(x => x.Code is not null);

        RuleFor(x => x.Remark)
            .MaximumLength(256).WithMessage("备注不能超过 256 个字符")
            .When(x => x.Remark is not null);
    }
}
