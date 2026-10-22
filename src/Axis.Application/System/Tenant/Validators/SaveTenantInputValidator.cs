using Axis.Application.System.Tenant.Dto;
using Axis.Domain.Enums;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.Tenant.Validators;

/// <summary>
/// <see cref="SaveTenantInput"/> 的入参格式校验。
/// </summary>
/// <remarks>
/// 仅做格式与必填；编码唯一性与独立库配置由 <see cref="ITenantAppService"/> 负责。
/// </remarks>
public sealed class SaveTenantInputValidator : Validator<SaveTenantInput>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public SaveTenantInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("租户名称不能为空")
            .MaximumLength(64).WithMessage("租户名称不能超过 64 个字符");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("租户编码不能为空")
            .MaximumLength(64).WithMessage("租户编码不能超过 64 个字符")
            .Matches(@"^[a-zA-Z][a-zA-Z0-9_]*$").WithMessage("租户编码须以字母开头，仅含字母数字下划线");

        RuleFor(x => x.Type)
            .Must(type => type is TenantTypeEnum.Id or TenantTypeEnum.Db)
            .WithMessage("隔离类型仅支持字段隔离或独立数据库");

        RuleFor(x => x.DbConfigId)
            .MaximumLength(64).WithMessage("数据库配置编号不能超过 64 个字符")
            .When(x => x.DbConfigId is not null);

        RuleFor(x => x.Remark)
            .MaximumLength(256).WithMessage("备注不能超过 256 个字符")
            .When(x => x.Remark is not null);
    }
}
