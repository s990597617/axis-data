using Axis.Application.System.Org.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.Org.Validators;

/// <summary>
/// <see cref="SaveOrgInput"/> 的入参格式校验。
/// </summary>
/// <remarks>
/// 仅做格式与必填；编码唯一性与树结构由 <see cref="IOrgAppService"/> 负责。
/// </remarks>
public sealed class SaveOrgInputValidator : Validator<SaveOrgInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public SaveOrgInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("机构名称不能为空")
            .MaximumLength(64).WithMessage("机构名称不能超过 64 个字符");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("机构编码不能为空")
            .MaximumLength(64).WithMessage("机构编码不能超过 64 个字符");

        RuleFor(x => x.Remark)
            .MaximumLength(256).WithMessage("备注不能超过 256 个字符")
            .When(x => x.Remark is not null);

        RuleFor(x => x.ParentId)
            .GreaterThanOrEqualTo(0).WithMessage("上级机构 Id 无效");
    }
}
