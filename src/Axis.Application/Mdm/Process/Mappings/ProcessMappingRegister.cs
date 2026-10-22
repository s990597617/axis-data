using Axis.Application.Mdm.Process.Dto;
using Axis.Domain.Mdm;
using Mapster;

namespace Axis.Application.Mdm.Process.Mappings;

/// <summary>
/// 工序入参到实体的映射：忽略主键与租户。
/// </summary>
public sealed class ProcessMappingRegister : IRegister
{
    /// <summary>
    /// 向全局 Mapster 配置注册工序映射。
    /// </summary>
    /// <param name="config">Mapster 全局配置。</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SaveProcessInput, MdmProcess>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.TenantId);
    }
}
