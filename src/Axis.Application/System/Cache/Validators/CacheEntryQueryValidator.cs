using Axis.Application.System.Cache.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.Cache.Validators;

/// <summary>
/// 缓存条目查询校验。
/// </summary>
public sealed class CacheEntryQueryValidator : Validator<CacheEntryQuery>
{
    public CacheEntryQueryValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("缓存键不能为空")
            .MaximumLength(500).WithMessage("缓存键过长");
    }
}
