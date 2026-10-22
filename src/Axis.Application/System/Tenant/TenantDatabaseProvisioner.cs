using System.Reflection;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Domain.System.Seeds;
using Axis.Infrastructure.Components.Database.Attributes;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Global;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace Axis.Application.System.Tenant;

/// <summary>
/// 独立库租户开通实现：校验连接配置后建库、InitTables，并写入非平台菜单种子。
/// </summary>
public sealed class TenantDatabaseProvisioner(
    ISqlSugarClient db,
    ILogger<TenantDatabaseProvisioner> logger)
    : ITenantDatabaseProvisioner, IAutoInject
{
    /// <inheritdoc />
    public async Task<TenantProvisionResult> ProvisionAsync(SysTenant tenant, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (tenant.Type != TenantTypeEnum.Db)
            return TenantProvisionResult.Ok("非独立库租户无需开通数据库");

        if (string.IsNullOrWhiteSpace(tenant.DbConfigId))
            return TenantProvisionResult.Fail("独立库租户缺少数据库配置编号");

        var config = GlobalContext.BootstrapDb?.ConnectionConfigs?
            .FirstOrDefault(item => string.Equals(
                item.ConfigId?.ToString(),
                tenant.DbConfigId,
                StringComparison.OrdinalIgnoreCase));

        if (config is null)
            return TenantProvisionResult.Fail($"数据库配置编号 '{tenant.DbConfigId}' 不存在");

        if (string.Equals(config.ConfigId?.ToString(), SqlSugarConst.MainConfigId, StringComparison.OrdinalIgnoreCase))
            return TenantProvisionResult.Fail("独立库不能使用主库配置编号 master");

        try
        {
            var tenantClient = (ITenant)db;
            var dbProvider = tenantClient.GetConnectionScope(config.ConfigId);

            if (config.DbSettings?.EnableInitDb == true && config.DbType != DbType.Oracle)
                dbProvider.DbMaintenance.CreateDatabase();

            if (config.DbSettings?.EnableInitTable == true)
                InitTables(dbProvider);

            if (config.DbSettings?.EnableInitSeed == true)
                await SeedTenantMenusAsync(dbProvider, ct);

            return TenantProvisionResult.Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "租户 {TenantCode} 独立库开通失败，ConfigId={ConfigId}", tenant.Code, tenant.DbConfigId);
            return TenantProvisionResult.Fail(ex.Message);
        }
    }

    private void InitTables(SqlSugarScopeProvider dbProvider)
    {
        var entityTypes = GlobalAssemblyLoader
            .LoadTypes(GlobalContext.BootstrapAssembly?.SqlSugar, logger)
            .Where(type => type is { IsInterface: false, IsAbstract: false, IsClass: true }
                           && type.IsDefined(typeof(SugarTable), false))
            .Where(type => type.GetCustomAttribute<TenantAttribute>() is null)
            .ToList();

        foreach (var entityType in entityTypes)
        {
            try
            {
                if (entityType.GetCustomAttribute<SplitTableAttribute>() is null)
                    dbProvider.CodeFirst.InitTables(entityType);
                else
                    dbProvider.CodeFirst.SplitTables().InitTables(entityType);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "租户库初始化表失败: {Entity}", entityType.FullName);
                throw;
            }
        }
    }

    private static async Task SeedTenantMenusAsync(SqlSugarScopeProvider dbProvider, CancellationToken ct)
    {
        var menus = new SysMenuSeed().HasData()
            .Where(menu => !menu.IsPlatform)
            .ToList();
        if (menus.Count == 0) return;

        var existing = await dbProvider.Queryable<SysMenu>().Select(m => m.Id).ToListAsync(ct);
        var existingSet = existing.ToHashSet();
        var toInsert = menus.Where(menu => !existingSet.Contains(menu.Id)).ToList();
        if (toInsert.Count == 0) return;

        await dbProvider.Insertable(toInsert).ExecuteCommandAsync(ct);
    }
}
