using Axis.Application.System.Dict.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.Dict.Validators;

/// <summary>
/// <see cref="DictItemsByCodeQuery"/> 入参校验。
/// </summary>
public sealed class DictItemsByCodeQueryValidator : Validator<DictItemsByCodeQuery>
{
    public DictItemsByCodeQueryValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("字典编码不能为空")
            .MaximumLength(64).WithMessage("字典编码最长 64 个字符");
    }
}
