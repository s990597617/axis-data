using Axis.Application.System.Cache.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.Cache.Validators;

/// <summary>
/// 按前缀清理缓存校验。
/// </summary>
public sealed class CachePrefixRequestValidator : Validator<CachePrefixRequest>
{
    public CachePrefixRequestValidator()
    {
        RuleFor(x => x.Prefix)
            .NotEmpty().WithMessage("缓存键前缀不能为空")
            .MaximumLength(250).WithMessage("缓存键前缀过长");
    }
}
