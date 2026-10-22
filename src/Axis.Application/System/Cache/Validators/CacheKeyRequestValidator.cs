using Axis.Application.System.Cache.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.Cache.Validators;

/// <summary>
/// 缓存键请求校验。
/// </summary>
public sealed class CacheKeyRequestValidator : Validator<CacheKeyRequest>
{
    public CacheKeyRequestValidator()
    {
        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("缓存键不能为空")
            .MaximumLength(500).WithMessage("缓存键过长");
    }
}
