using Axis.Application.System.CodeRule.Dto;
using Axis.Domain.System;
using Mapster;

namespace Axis.Application.System.CodeRule.Mappings;

/// <summary>
/// 编码规则入参到实体的映射：忽略主键、租户与内置标记。
/// </summary>
public sealed class CodeRuleMappingRegister : IRegister
{
    /// <summary>
    /// 向全局 Mapster 配置注册编码规则映射。
    /// </summary>
    /// <param name="config">Mapster 全局配置。</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SaveCodeRuleInput, SysCodeRule>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.IsBuiltin);
    }
}
