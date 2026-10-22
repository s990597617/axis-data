using Axis.Application.Mdm.Material.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.Mdm.Material.Validators;

/// <summary>
/// <see cref="SaveMaterialInput"/> 的入参格式校验。
/// </summary>
public sealed class SaveMaterialInputValidator : Validator<SaveMaterialInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public SaveMaterialInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("物料名称不能为空")
            .MaximumLength(128).WithMessage("物料名称不能超过 128 个字符");

        RuleFor(x => x.Code)
            .MaximumLength(64).WithMessage("物料编码不能超过 64 个字符")
            .When(x => x.Code is not null);

        RuleFor(x => x.Spec)
            .MaximumLength(512).WithMessage("规格型号不能超过 512 个字符")
            .When(x => x.Spec is not null);

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("计量单位不能为空")
            .MaximumLength(32).WithMessage("计量单位不能超过 32 个字符");

        RuleFor(x => x.SourceType)
            .NotEmpty().WithMessage("来源类型不能为空")
            .MaximumLength(32).WithMessage("来源类型不能超过 32 个字符");

        RuleFor(x => x.Remark)
            .MaximumLength(256).WithMessage("备注不能超过 256 个字符")
            .When(x => x.Remark is not null);
    }
}
