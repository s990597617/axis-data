using Axis.Application.Mdm.Material.Dto;
using Axis.Domain.Mdm;
using Mapster;

namespace Axis.Application.Mdm.Material.Mappings;

/// <summary>
/// 物料入参到实体的映射：忽略主键与租户。
/// </summary>
public sealed class MaterialMappingRegister : IRegister
{
    /// <summary>
    /// 向全局 Mapster 配置注册物料映射。
    /// </summary>
    /// <param name="config">Mapster 全局配置。</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SaveMaterialInput, MdmMaterial>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId);
    }
}
