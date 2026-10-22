using Axis.Application.Demo.Brand.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.Demo.Brand.Validators;

/// <summary>
/// <see cref="SaveBrandInput"/> 的入参格式校验。
/// </summary>
public sealed class SaveBrandInputValidator : Validator<SaveBrandInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public SaveBrandInputValidator()
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
