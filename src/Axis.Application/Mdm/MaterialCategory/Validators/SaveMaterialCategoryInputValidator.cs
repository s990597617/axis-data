using Axis.Application.Mdm.MaterialCategory.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.Mdm.MaterialCategory.Validators;

/// <summary>
/// <see cref="SaveMaterialCategoryInput"/> 的入参格式校验。
/// </summary>
/// <remarks>
/// 仅做格式与必填；编码唯一性与树结构由 <see cref="IMaterialCategoryAppService"/> 负责。
/// </remarks>
public sealed class SaveMaterialCategoryInputValidator : Validator<SaveMaterialCategoryInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public SaveMaterialCategoryInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("分类名称不能为空")
            .MaximumLength(64).WithMessage("分类名称不能超过 64 个字符");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("分类编码不能为空")
            .MaximumLength(64).WithMessage("分类编码不能超过 64 个字符");

        RuleFor(x => x.Remark)
            .MaximumLength(256).WithMessage("备注不能超过 256 个字符")
            .When(x => x.Remark is not null);

        RuleFor(x => x.ParentId)
            .GreaterThanOrEqualTo(0).WithMessage("上级分类 Id 无效");
    }
}
