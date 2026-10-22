using System.Globalization;
using System.Text.RegularExpressions;
using Axis.Application.Common;
using Axis.Application.System.User.Dto;
using Axis.Domain.Enums;
using Axis.Domain.System;
using Axis.Infrastructure.Author.Internal;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.System.User;

/// <summary>
/// 用户 Excel 导入导出扩展。
/// </summary>
public sealed partial class UserAppService
{
    private const int MaxImportRows = 2000;
    private const int MaxExportRows = 10000;

    /// <summary>
    /// 下载用户导入 Excel 模板（仅表头与空行）。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    public Task<ExcelFileResult> GetImportTemplateAsync(CancellationToken ct = default)
    {
        var bytes = excel.Export(Array.Empty<UserImportRow>());
        return Task.FromResult(new ExcelFileResult
        {
            Content = bytes,
            FileName = $"用户导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
        });
    }

    /// <summary>
    /// 按模板批量导入用户；账号已存在的行记为失败，不中断整批。
    /// </summary>
    /// <param name="stream">xlsx 文件流。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task<ImportResultDto> ImportAsync(Stream stream, CancellationToken ct = default)
    {
        var rows = excel.Query<UserImportRow>(stream);
        if (rows.Count > MaxImportRows)
            throw new BusinessException($"单次最多导入 {MaxImportRows} 行");

        var orgCodes = rows
            .Select(item => item.OrgCode?.Trim())
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var posCodes = rows
            .Select(item => item.PosCode?.Trim())
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var roleCodes = rows
            .SelectMany(item => SplitCodes(item.RoleCodes))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var orgs = orgCodes.Length == 0
            ? new Dictionary<string, SysOrg>(StringComparer.OrdinalIgnoreCase)
            : (await Db.Queryable<SysOrg>()
                    .Where(item => orgCodes.Contains(item.Code))
                    .ToListAsync(ct))
                .Where(item => !string.IsNullOrWhiteSpace(item.Code))
                .GroupBy(item => item.Code!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var positions = posCodes.Length == 0
            ? new Dictionary<string, SysPos>(StringComparer.OrdinalIgnoreCase)
            : (await Db.Queryable<SysPos>()
                    .Where(item => posCodes.Contains(item.Code))
                    .ToListAsync(ct))
                .Where(item => !string.IsNullOrWhiteSpace(item.Code))
                .GroupBy(item => item.Code!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var roles = roleCodes.Length == 0
            ? new Dictionary<string, SysRole>(StringComparer.OrdinalIgnoreCase)
            : (await Db.Queryable<SysRole>()
                    .Where(item => roleCodes.Contains(item.Code))
                    .ToListAsync(ct))
                .GroupBy(item => item.Code, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

        var result = new ImportResultDto();
        for (var index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            var excelRow = index + 2;
            if (IsBlankRow(row))
                continue;

            try
            {
                await ImportOneAsync(row, orgs, positions, roles, ct);
                result.SuccessCount++;
            }
            catch (Exception ex)
            {
                result.Failures.Add(new ImportFailureDto
                {
                    Row = excelRow,
                    Message = ex is BusinessException be ? be.Message : "导入失败"
                });
            }
        }

        result.FailureCount = result.Failures.Count;
        return result;
    }

    /// <summary>
    /// 按与列表相同的筛选条件导出用户 Excel（最多 10000 行）。
    /// </summary>
    /// <param name="query">筛选条件；分页参数忽略。</param>
    /// <param name="ct">取消令牌。</param>
    public async Task<ExcelFileResult> ExportAsync(UserPageQuery query, CancellationToken ct = default)
    {
        var paged = await userRepo.PageWithOrgAsync(
            1, MaxExportRows, query.OrgId, query.Keyword, query.Status, ct);

        var userIds = paged.Items.Select(item => item.Id).ToArray();
        var roleMap = new Dictionary<long, List<string>>();
        if (userIds.Length > 0)
        {
            var relations = await Db.Queryable<SysUserRole, SysRole>((ur, role) => ur.RoleId == role.Id)
                .Where((ur, role) => userIds.Contains(ur.UserId))
                .Select((ur, role) => new { ur.UserId, role.Code })
                .ToListAsync(ct);
            foreach (var group in relations.GroupBy(item => item.UserId))
                roleMap[group.Key] = group.Select(item => item.Code).Where(code => !string.IsNullOrWhiteSpace(code)).ToList();
        }

        var posIds = paged.Items.Select(item => item.PosId).Where(id => id > 0).Distinct().ToArray();
        var posCodeMap = posIds.Length == 0
            ? new Dictionary<long, string>()
            : (await Db.Queryable<SysPos>().Where(item => posIds.Contains(item.Id)).ToListAsync(ct))
                .ToDictionary(item => item.Id, item => item.Code ?? string.Empty);

        var exportRows = paged.Items.Select(user => new UserExportRow
        {
            OrgCode = user.SysOrg?.Code,
            OrgName = user.SysOrg?.Name,
            Account = user.Account,
            RealName = user.RealName,
            NickName = user.NickName,
            Phone = user.Phone,
            Email = user.Email,
            Sex = FormatSex(user.Sex),
            JobNum = user.JobNum,
            JoinDate = user.JoinDate?.ToString("yyyy-MM-dd"),
            PosCode = user.PosId > 0 && posCodeMap.TryGetValue(user.PosId, out var posCode) ? posCode : null,
            Status = user.Status == StatusEnum.Enable ? "启用" : "禁用",
            OrderNo = user.OrderNo,
            RoleCodes = roleMap.TryGetValue(user.Id, out var codes) ? string.Join(',', codes) : null,
            Remark = user.Remark
        }).ToList();

        return new ExcelFileResult
        {
            Content = excel.Export(exportRows),
            FileName = $"用户导出_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
        };
    }

    private async Task ImportOneAsync(
        UserImportRow row,
        IReadOnlyDictionary<string, SysOrg> orgs,
        IReadOnlyDictionary<string, SysPos> positions,
        IReadOnlyDictionary<string, SysRole> roles,
        CancellationToken ct)
    {
        var orgCode = row.OrgCode?.Trim();
        var account = row.Account?.Trim();
        var realName = row.RealName?.Trim();
        if (string.IsNullOrWhiteSpace(orgCode))
            throw new BusinessException("机构编码不能为空");
        if (string.IsNullOrWhiteSpace(account))
            throw new BusinessException("账号不能为空");
        if (!Regex.IsMatch(account, @"^[a-zA-Z0-9_]+$"))
            throw new BusinessException("账号仅允许字母、数字与下划线");
        if (account.Length > 32)
            throw new BusinessException("账号不能超过 32 个字符");
        if (string.IsNullOrWhiteSpace(realName))
            throw new BusinessException("姓名不能为空");

        if (!orgs.TryGetValue(orgCode, out var org))
            throw new BusinessException($"机构编码「{orgCode}」不存在或无权访问");

        await EnsureOrgAccessibleAsync(org.Id, ct);

        var exists = await Repo.AsQueryable().AnyAsync(u => u.Account == account, ct);
        if (exists) throw new BusinessException($"账号「{account}」已存在");

        long posId = 0;
        var posCode = row.PosCode?.Trim();
        if (!string.IsNullOrWhiteSpace(posCode))
        {
            if (!positions.TryGetValue(posCode, out var pos))
                throw new BusinessException($"岗位编码「{posCode}」不存在");
            posId = pos.Id;
        }

        var roleIds = new List<long>();
        foreach (var code in SplitCodes(row.RoleCodes))
        {
            if (!roles.TryGetValue(code, out var role))
                throw new BusinessException($"角色编码「{code}」不存在");
            roleIds.Add(role.Id);
        }

        var nickName = string.IsNullOrWhiteSpace(row.NickName) ? realName : row.NickName.Trim();
        var input = new CreateUserInput
        {
            OrgId = org.Id,
            PosId = posId,
            Account = account,
            RealName = realName,
            NickName = nickName,
            Phone = NullIfWhiteSpace(row.Phone),
            Email = NullIfWhiteSpace(row.Email),
            Sex = ParseSex(row.Sex),
            JobNum = NullIfWhiteSpace(row.JobNum),
            JoinDate = ParseDate(row.JoinDate),
            Status = ParseStatus(row.Status),
            OrderNo = ParseOrderNo(row.OrderNo),
            AccountType = AccountTypeEnum.User,
            Remark = NullIfWhiteSpace(row.Remark),
            RoleIds = roleIds
        };

        await CreateAsync(input, ct);
    }

    private static bool IsBlankRow(UserImportRow row)
        => string.IsNullOrWhiteSpace(row.OrgCode)
           && string.IsNullOrWhiteSpace(row.Account)
           && string.IsNullOrWhiteSpace(row.RealName);

    private static IEnumerable<string> SplitCodes(string? raw)
        => (raw ?? string.Empty)
            .Split([',', '，', ';', '；', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static GenderEnum ParseSex(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return GenderEnum.Unknown;
        var text = raw.Trim();
        if (text is "1" or "男") return GenderEnum.Male;
        if (text is "2" or "女") return GenderEnum.Female;
        if (text is "0" or "未知") return GenderEnum.Unknown;
        throw new BusinessException("性别仅支持：未知/男/女 或 0/1/2");
    }

    private static StatusEnum ParseStatus(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return StatusEnum.Enable;
        var text = raw.Trim();
        if (text is "1" or "启用") return StatusEnum.Enable;
        if (text is "0" or "禁用") return StatusEnum.Disable;
        throw new BusinessException("状态仅支持：启用/禁用 或 1/0");
    }

    private static int ParseOrderNo(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return 100;
        if (!int.TryParse(raw.Trim(), out var orderNo) || orderNo is < 0 or > 9999)
            throw new BusinessException("排序须为 0-9999 的整数");
        return orderNo;
    }

    private static DateTime? ParseDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        if (DateTime.TryParse(raw.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            || DateTime.TryParse(raw.Trim(), out date))
            return date.Date;
        throw new BusinessException("入职日期格式不正确");
    }

    private static string FormatSex(GenderEnum sex) => sex switch
    {
        GenderEnum.Male => "男",
        GenderEnum.Female => "女",
        _ => "未知"
    };
}
