using Axis.Application.System.Notice.Dto;
using Axis.Domain.Enums;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.Notice.Validators;

/// <summary>
/// <see cref="SaveNoticeInput"/> 的入参格式校验。
/// </summary>
/// <remarks>
/// 仅做格式与必填；目标用户是否真实存在由发布时仓储解析，不在此校验。
/// </remarks>
public sealed class SaveNoticeInputValidator : Validator<SaveNoticeInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public SaveNoticeInputValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.TargetIds).MaximumLength(2000).When(x => x.TargetIds is not null);
        RuleFor(x => x.Remark).MaximumLength(256).When(x => x.Remark is not null);
        RuleFor(x => x.OrderNo).InclusiveBetween(0, 9999);
        RuleFor(x => x.TargetIds)
            .NotEmpty()
            .When(x => x.TargetType != NoticeTargetTypeEnum.All)
            .WithMessage("非全部范围时必须指定目标");
    }
}
