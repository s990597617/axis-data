using Axis.Application.System.Dict.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.Dict.Validators;

/// <summary>
/// <see cref="SaveDictTypeInput"/> 的入参格式校验。
/// </summary>
/// <remarks>
/// 仅做格式与必填；编码唯一性由 <see cref="IDictAppService"/> 负责。
/// </remarks>
public sealed class SaveDictTypeInputValidator : Validator<SaveDictTypeInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public SaveDictTypeInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("字典名称不能为空")
            .MaximumLength(64).WithMessage("字典名称不能超过 64 个字符");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("字典编码不能为空")
            .MaximumLength(64).WithMessage("字典编码不能超过 64 个字符")
            .Matches(@"^[a-zA-Z][a-zA-Z0-9_]*$").WithMessage("字典编码须以字母开头，仅含字母数字下划线");

        RuleFor(x => x.Remark)
            .MaximumLength(256).WithMessage("备注不能超过 256 个字符")
            .When(x => x.Remark is not null);
    }
}
