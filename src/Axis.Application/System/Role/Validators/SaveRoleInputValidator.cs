using Axis.Application.System.Role.Dto;
using Axis.Domain.Enums;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.Role.Validators;

/// <summary>
/// <see cref="SaveRoleInput"/> 的入参格式校验。
/// </summary>
/// <remarks>
/// 仅做格式与必填；业务唯一性由 <see cref="IRoleAppService"/> 负责。
/// </remarks>
public sealed class SaveRoleInputValidator : Validator<SaveRoleInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public SaveRoleInputValidator()
    {
        RuleFor(x => x.Version)
            .GreaterThanOrEqualTo(0).WithMessage("版本号无效")
            .When(x => x.Id > 0);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("角色名称不能为空")
            .MaximumLength(64).WithMessage("角色名称不能超过 64 个字符");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("角色编码不能为空")
            .MaximumLength(64).WithMessage("角色编码不能超过 64 个字符")
            .Matches(@"^[a-zA-Z][a-zA-Z0-9_]*$").WithMessage("角色编码须以字母开头，仅含字母数字下划线");

        RuleFor(x => x.DataScope)
            .IsInEnum().WithMessage("请选择有效的数据范围");

        RuleFor(x => x.OrgIds)
            .Must(ids => ids is { Count: > 0 })
            .WithMessage("自定义数据范围须选择至少一个机构")
            .When(x => x.DataScope == DataScopeEnum.Custom);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("请选择有效的状态");

        RuleFor(x => x.OrderNo)
            .InclusiveBetween(0, 9999).WithMessage("排序须在 0-9999 之间");

        RuleFor(x => x.Remark)
            .MaximumLength(256).WithMessage("备注不能超过 256 个字符")
            .When(x => x.Remark is not null);
    }
}
