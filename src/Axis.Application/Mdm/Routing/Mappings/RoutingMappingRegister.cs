using Axis.Application.Mdm.Routing.Dto;
using Axis.Domain.Mdm;
using Mapster;

namespace Axis.Application.Mdm.Routing.Mappings;

/// <summary>
/// 工艺路线入参到实体的映射：忽略主键与租户。
/// </summary>
public sealed class RoutingMappingRegister : IRegister
{
    /// <summary>
    /// 向全局 Mapster 配置注册工艺路线映射。
    /// </summary>
    /// <param name="config">Mapster 全局配置。</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SaveRoutingInput, MdmRouting>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId);
    }
}
