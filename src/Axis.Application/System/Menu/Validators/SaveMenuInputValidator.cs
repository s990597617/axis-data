using Axis.Application.System.Menu.Dto;
using Axis.Domain.Enums;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.Menu.Validators;

/// <summary>
/// <see cref="SaveMenuInput"/> 的入参格式校验。
/// </summary>
/// <remarks>
/// 仅做格式与必填；类型相关业务规则与唯一性由 <see cref="IMenuAppService"/> 负责。
/// </remarks>
public sealed class SaveMenuInputValidator : Validator<SaveMenuInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public SaveMenuInputValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("请选择有效的节点类型");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("菜单标题不能为空")
            .MaximumLength(64).WithMessage("菜单标题不能超过 64 个字符");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("路由名称不能为空")
            .MaximumLength(64).WithMessage("路由名称不能超过 64 个字符")
            .When(x => x.Type is MenuTypeEnum.Dir or MenuTypeEnum.Menu);

        RuleFor(x => x.Path)
            .NotEmpty().WithMessage("路由路径不能为空")
            .MaximumLength(128).WithMessage("路由路径不能超过 128 个字符")
            .When(x => x.Type is MenuTypeEnum.Dir or MenuTypeEnum.Menu);

        RuleFor(x => x.Component)
            .NotEmpty().WithMessage("组件路径不能为空")
            .MaximumLength(128).WithMessage("组件路径不能超过 128 个字符")
            .When(x => x.Type == MenuTypeEnum.Menu);

        RuleFor(x => x.Permission)
            .MaximumLength(128).WithMessage("权限标识不能超过 128 个字符")
            .When(x => x.Permission is not null);

        RuleFor(x => x.Icon)
            .NotEmpty().WithMessage("菜单图标不能为空")
            .MaximumLength(64).WithMessage("图标不能超过 64 个字符")
            .When(x => x.Type is MenuTypeEnum.Dir or MenuTypeEnum.Menu);

        RuleFor(x => x.Redirect)
            .MaximumLength(256).WithMessage("重定向地址不能超过 256 个字符")
            .When(x => x.Redirect is not null);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("请选择有效的状态");

        RuleFor(x => x.OrderNo)
            .InclusiveBetween(0, 9999).WithMessage("排序须在 0-9999 之间");

        RuleFor(x => x.Remark)
            .MaximumLength(256).WithMessage("备注不能超过 256 个字符")
            .When(x => x.Remark is not null);

        RuleFor(x => x.ParentId)
            .GreaterThanOrEqualTo(0).WithMessage("父级菜单 Id 无效");
    }
}
