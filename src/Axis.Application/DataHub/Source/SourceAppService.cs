using Axis.Application.Common;
using Axis.Application.DataHub.Common;
using Axis.Application.DataHub.Source.Dto;
using Axis.Domain.DataHub;
using Axis.Domain.Enums;
using Axis.Infrastructure.Author;
using Axis.Infrastructure.Components.Database;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Components.Database.Repositories;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using SqlSugar;
using Yitter.IdGenerator;

namespace Axis.Application.DataHub.Source;

/// <summary>
/// 数据源管理应用服务。
/// </summary>
public sealed class SourceAppService(
    IRepository<DhSource> repository,
    ISqlSugarClient db,
    ICurrentAccount currentAccount,
    SourceConnectionFactory connectionFactory)
    : BaseAppService<DhSource>(repository, db, currentAccount), ISourceAppService, IAutoInject
{
    public async Task<SqlSugarPagedList<SourceDto>> PageAsync(SourcePageQuery query, CancellationToken ct = default)
        => await Repo.AsQueryable()
            .WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item => item.Name.Contains(query.Keyword!) || (item.Code != null && item.Code.Contains(query.Keyword!)))
            .WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)
            .OrderBy(item => item.OrderNo)
            .OrderBy(item => item.CreatedTime, OrderByType.Desc)
            .Select(item => new SourceDto
            {
                Id = item.Id,
                Name = item.Name,
                Code = item.Code,
                SourceType = item.SourceType,
                SecretRef = item.SecretRef,
                HasConnectionString = item.ConnectionString != null,
                LastTestStatus = item.LastTestStatus,
                LastTestTime = item.LastTestTime,
                LastTestMessage = item.LastTestMessage,
                IsReadOnly = item.IsReadOnly,
                OrderNo = item.OrderNo,
                Status = item.Status,
                Remark = item.Remark,
                CreatedTime = item.CreatedTime,
                ModifiedTime = item.ModifiedTime
            })
            .ToPagedListAsync(query.Page, query.PageSize);

    public async Task<IReadOnlyList<DataHubLookupItemDto>> ListLookupAsync(CancellationToken ct = default)
    {
        var rows = await Repo.AsQueryable()
            .Where(item => item.Status == StatusEnum.Enable)
            .WhereIF(CurrentAccount.TenantId.HasValue, item => item.TenantId == CurrentAccount.TenantId)
            .OrderBy(item => item.OrderNo)
            .OrderBy(item => item.Name)
            .Select(item => new { item.Id, item.Name, item.Code })
            .ToListAsync(ct);

        return rows.Select(item => new DataHubLookupItemDto
        {
            Value = item.Id.ToString(),
            Label = string.IsNullOrWhiteSpace(item.Code) ? item.Name : $"{item.Name}（{item.Code}）"
        }).ToList();
    }

    public async Task CreateAsync(SaveSourceInput input, CancellationToken ct = default)
    {
        await EnsureCodeUniqueAsync(input.Code, null, ct);

        var entity = new DhSource
        {
            Id = YitIdHelper.NextId(),
            TenantId = CurrentAccount.TenantId,
            Name = input.Name,
            Code = input.Code,
            SourceType = input.SourceType,
            SecretRef = NormalizeOptional(input.SecretRef),
            ConnectionString = NormalizeOptional(input.ConnectionString),
            IsReadOnly = input.IsReadOnly,
            OrderNo = input.OrderNo,
            Status = input.Status,
            Remark = input.Remark,
            LastTestStatus = DhConnectionTestStatusEnum.Unknown,
            CreatedBy = CurrentAccount.Id,
            Creator = CurrentAccount.RealName,
            CreatedTime = DateTime.Now
        };

        EnsureConnectionConfigured(entity);
        await Db.Insertable(entity).ExecuteCommandAsync(ct);
    }

    public async Task UpdateAsync(long id, SaveSourceInput input, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("数据源不存在");
        await EnsureCodeUniqueAsync(input.Code, id, ct);

        entity.Name = input.Name;
        entity.Code = input.Code;
        entity.SourceType = input.SourceType;
        entity.SecretRef = NormalizeOptional(input.SecretRef);
        ApplyConnectionStringUpdate(entity, input.ConnectionString);
        entity.IsReadOnly = input.IsReadOnly;
        entity.OrderNo = input.OrderNo;
        entity.Status = input.Status;
        entity.Remark = input.Remark;
        entity.ModifiedBy = CurrentAccount.Id;
        entity.Modifier = CurrentAccount.RealName;
        entity.ModifiedTime = DateTime.Now;

        EnsureConnectionConfigured(entity);
        await Repo.AsUpdateable(entity).ExecuteWithOptLockAsync(ct);
    }

    public override async Task DeleteAsync(long id, CancellationToken ct = default)
    {
        var inUse = await Db.Queryable<DhDataset>().Where(item => item.SourceId == id).AnyAsync(ct);
        if (inUse) throw new BusinessException("数据源已被数据集引用，无法删除");

        var affected = await Db.Deleteable<DhSource>().Where(item => item.Id == id).ExecuteCommandAsync(ct);
        if (affected == 0) throw new BusinessException("数据源不存在");
    }

    public Task<TestSourceConnectionResultDto> TestConnectionAsync(
        TestSourceConnectionInput input,
        CancellationToken ct = default)
        => connectionFactory.TestAsync(input, ct);

    public async Task<TestSourceConnectionResultDto> TestConnectionByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await Repo.AsQueryable().Where(item => item.Id == id).FirstAsync(ct)
                     ?? throw new BusinessException("数据源不存在");

        var input = new TestSourceConnectionInput
        {
            SourceType = entity.SourceType,
            SecretRef = entity.SecretRef,
            ConnectionString = entity.ConnectionString
        };

        var result = await connectionFactory.TestAsync(input, ct);

        entity.LastTestStatus = result.Success
            ? DhConnectionTestStatusEnum.Success
            : DhConnectionTestStatusEnum.Failed;
        entity.LastTestTime = DateTime.Now;
        entity.LastTestMessage = TrimStoredMessage(result.Message);
        entity.ModifiedBy = CurrentAccount.Id;
        entity.Modifier = CurrentAccount.RealName;
        entity.ModifiedTime = DateTime.Now;
        await Repo.AsUpdateable(entity).ExecuteCommandAsync(ct);

        return result;
    }

    private static void ApplyConnectionStringUpdate(DhSource entity, string? connectionString)
    {
        // null/省略：保留；空字符串：清空；非空：替换
        if (connectionString is null) return;
        entity.ConnectionString = string.IsNullOrWhiteSpace(connectionString)
            ? null
            : connectionString.Trim();
    }

    private static void EnsureConnectionConfigured(DhSource entity)
    {
        if (entity.Status != StatusEnum.Enable) return;
        if (string.IsNullOrWhiteSpace(entity.SecretRef) && string.IsNullOrWhiteSpace(entity.ConnectionString))
            throw new BusinessException("启用状态的数据源必须配置密钥引用或连接串");
    }

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? TrimStoredMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message)) return null;
        message = message.ReplaceLineEndings(" ").Trim();
        return message.Length > 512 ? message[..512] : message;
    }

    private async Task EnsureCodeUniqueAsync(string? code, long? excludedId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code)) return;
        var exists = await Repo.AsQueryable()
            .Where(item => item.Code == code)
            .WhereIF(CurrentAccount.TenantId.HasValue, item => item.TenantId == CurrentAccount.TenantId)
            .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
            .AnyAsync(ct);
        if (exists) throw new BusinessException($"数据源编码 '{code}' 已存在");
    }
}