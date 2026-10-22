using Axis.Application.System.User.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.User.Validators;

/// <summary>
/// <see cref="UpdateUserInput"/> 的入参格式校验。
/// </summary>
public sealed class UpdateUserInputValidator : Validator<UpdateUserInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public UpdateUserInputValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("用户 Id 无效");

        RuleFor(x => x.Version)
            .GreaterThanOrEqualTo(0).WithMessage("版本号无效");

        RuleFor(x => x.OrgId)
            .GreaterThan(0).WithMessage("所属机构不能为空");

        RuleFor(x => x.RealName)
            .NotEmpty().WithMessage("真实姓名不能为空")
            .MaximumLength(32).WithMessage("真实姓名不能超过 32 个字符");

        RuleFor(x => x.NickName)
            .NotEmpty().WithMessage("昵称不能为空")
            .MaximumLength(32).WithMessage("昵称不能超过 32 个字符");

        RuleFor(x => x.Avatar)
            .MaximumLength(512).WithMessage("头像地址不能超过 512 个字符")
            .When(x => x.Avatar is not null);

        RuleFor(x => x.AccountType)
            .IsInEnum().WithMessage("请选择有效的账号类型");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("请选择有效的状态");

        RuleFor(x => x.OrderNo)
            .InclusiveBetween(0, 9999).WithMessage("排序须在 0-9999 之间");

        RuleFor(x => x.Phone)
            .MaximumLength(16).WithMessage("手机号不能超过 16 个字符")
            .When(x => x.Phone is not null);

        RuleFor(x => x.Email)
            .MaximumLength(64).WithMessage("邮箱不能超过 64 个字符")
            .EmailAddress().WithMessage("邮箱格式不正确")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.IdCardNum)
            .MaximumLength(32).WithMessage("身份证号不能超过 32 个字符")
            .When(x => x.IdCardNum is not null);

        RuleFor(x => x.JobNum)
            .MaximumLength(32).WithMessage("工号不能超过 32 个字符")
            .When(x => x.JobNum is not null);

        RuleFor(x => x.Remark)
            .MaximumLength(256).WithMessage("备注不能超过 256 个字符")
            .When(x => x.Remark is not null);
    }
}
