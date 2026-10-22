using Axis.Application.System.Dict.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.Dict.Validators;

/// <summary>
/// <see cref="SaveDictDataInput"/> 的入参格式校验。
/// </summary>
/// <remarks>
/// 仅做格式与必填；类型存在性与 Value 唯一性由 <see cref="IDictAppService"/> 负责。
/// </remarks>
public sealed class SaveDictDataInputValidator : Validator<SaveDictDataInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public SaveDictDataInputValidator()
    {
        RuleFor(x => x.DictTypeId)
            .GreaterThan(0).WithMessage("字典类型 Id 无效");

        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("字典标签不能为空")
            .MaximumLength(64).WithMessage("字典标签不能超过 64 个字符");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("字典值不能为空")
            .MaximumLength(64).WithMessage("字典值不能超过 64 个字符");

        RuleFor(x => x.TagStyle)
            .MaximumLength(32).WithMessage("标签样式不能超过 32 个字符")
            .When(x => x.TagStyle is not null);

        RuleFor(x => x.ListClass)
            .MaximumLength(32).WithMessage("列表样式不能超过 32 个字符")
            .When(x => x.ListClass is not null);

        RuleFor(x => x.Remark)
            .MaximumLength(256).WithMessage("备注不能超过 256 个字符")
            .When(x => x.Remark is not null);
    }
}
