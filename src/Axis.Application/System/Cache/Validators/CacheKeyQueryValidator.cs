using Axis.Application.System.Cache.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.Cache.Validators;

/// <summary>
/// 按前缀查询缓存键校验。
/// </summary>
public sealed class CacheKeyQueryValidator : Validator<CacheKeyQuery>
{
    public CacheKeyQueryValidator()
    {
        RuleFor(x => x.Prefix)
            .NotEmpty().WithMessage("缓存键前缀不能为空")
            .MaximumLength(250).WithMessage("缓存键前缀过长");

        RuleFor(x => x.Take)
            .InclusiveBetween(1, 500).WithMessage("返回数量须在 1~500 之间");
    }
}
