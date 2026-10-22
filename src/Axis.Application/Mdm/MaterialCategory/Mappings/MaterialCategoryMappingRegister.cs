using Axis.Application.Mdm.MaterialCategory.Dto;
using Axis.Domain.Mdm;
using Mapster;

namespace Axis.Application.Mdm.MaterialCategory.Mappings;

/// <summary>
/// 物料分类入参到实体的映射：忽略主键与租户。
/// </summary>
public sealed class MaterialCategoryMappingRegister : IRegister
{
    /// <summary>
    /// 向全局 Mapster 配置注册物料分类映射。
    /// </summary>
    /// <param name="config">Mapster 全局配置。</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SaveMaterialCategoryInput, MdmMaterialCategory>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId);
    }
}
