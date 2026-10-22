using Axis.Application.System.User.Dto;
using Axis.Domain.System;
using Mapster;

namespace Axis.Application.System.User.Mappings;

/// <summary>
/// 用户模块对象映射注册：创建 / 更新入参到实体、实体到列表 / 详情 DTO。
/// </summary>
/// <remarks>
/// 同名字段由全局 Flexible 策略自动匹配；密码等敏感字段与服务端字段须 Ignore，由用例内单独赋值。
/// </remarks>
public sealed class UserMappingRegister : IRegister
{
    /// <summary>
    /// 向全局 <see cref="TypeAdapterConfig"/> 注册用户相关映射。
    /// </summary>
    /// <param name="config">Mapster 全局配置实例，由启动扫描传入。</param>
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateUserInput, SysUser>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Password)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.Version)
            .Ignore(dest => dest.LastLoginIp)
            .Ignore(dest => dest.LastLoginTime)
            .Ignore(dest => dest.LastLoginDevice)
            .Map(dest => dest.Birthday, src => src.Birthday ?? default);

        config.NewConfig<UpdateUserInput, SysUser>()
            .Ignore(dest => dest.Account)
            .Ignore(dest => dest.Password)
            .Ignore(dest => dest.TenantId)
            .Ignore(dest => dest.CreatedBy)
            .Ignore(dest => dest.Creator)
            .Ignore(dest => dest.CreatedTime)
            .Ignore(dest => dest.Deleted)
            .Ignore(dest => dest.LastLoginIp)
            .Ignore(dest => dest.LastLoginTime)
            .Ignore(dest => dest.LastLoginDevice)
            .Ignore(dest => dest.SysOrg)
            .Map(dest => dest.Birthday, src => src.Birthday ?? default);

        config.NewConfig<SysUser, UserDto>()
            .Map(dest => dest.OrgName, src => src.SysOrg != null ? src.SysOrg.Name : string.Empty);
    }
}
