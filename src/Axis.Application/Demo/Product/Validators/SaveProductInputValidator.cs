using Axis.Application.Demo.Product.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.Demo.Product.Validators;

/// <summary>
/// <see cref="SaveProductInput"/> 的入参格式校验。
/// </summary>
public sealed class SaveProductInputValidator : Validator<SaveProductInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public SaveProductInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("产品名称不能为空")
            .MaximumLength(64).WithMessage("产品名称不能超过 64 个字符");

        RuleFor(x => x.Code)
            .MaximumLength(64).WithMessage("产品编码不能超过 64 个字符")
            .When(x => x.Code is not null);

        RuleFor(x => x.Remark)
            .MaximumLength(256).WithMessage("备注不能超过 256 个字符")
            .When(x => x.Remark is not null);
    }
}
