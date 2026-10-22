using Axis.Application.Common;
using Axis.Application.System.User.Dto;
using Axis.Domain.Enums;
using Axis.Infrastructure.Components.Database.Internal;
using Axis.Infrastructure.Exceptions;

namespace Axis.Application.System.User;

/// <summary>
/// 用户管理应用服务契约。
/// </summary>
/// <remarks>
/// 负责用户主表维护；机构树查询归 <c>IOrgAppService</c>，登录会话归 Author 模块。
/// </remarks>
public interface IUserAppService
{
    /// <summary>
    /// 按条件分页查询用户列表。
    /// </summary>
    /// <param name="query">分页与筛选条件；关键字匹配账号、姓名、昵称、手机号。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>用户 DTO 分页结果（含机构名称）。</returns>
    Task<SqlSugarPagedList<UserDto>> PageAsync(UserPageQuery query, CancellationToken ct = default);

    /// <summary>
    /// 按主键获取用户详情。
    /// </summary>
    /// <param name="id">用户主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <returns>用户 DTO；不存在时为 null。</returns>
    Task<UserDto?> GetAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 校验账号唯一后创建用户；初始密码由系统内置默认值哈希落库。
    /// </summary>
    /// <param name="input">创建用户字段（不含密码）。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">账号已存在时抛出。</exception>
    Task CreateAsync(CreateUserInput input, CancellationToken ct = default);

    /// <summary>
    /// 更新用户资料（不含账号与密码）。
    /// </summary>
    /// <param name="input">更新字段；须包含有效用户 Id。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">用户不存在时抛出。</exception>
    Task UpdateAsync(UpdateUserInput input, CancellationToken ct = default);

    /// <summary>
    /// 删除指定用户。
    /// </summary>
    /// <param name="id">用户主键。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">用户不存在时抛出。</exception>
    Task DeleteAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// 重置用户密码并以 BCrypt 哈希后落库。
    /// </summary>
    /// <param name="id">用户主键。</param>
    /// <param name="newPassword">新密码明文。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">用户不存在时抛出。</exception>
    Task ResetPasswordAsync(long id, string newPassword, CancellationToken ct = default);

    /// <summary>
    /// 修改用户启用状态。
    /// </summary>
    /// <param name="id">用户主键。</param>
    /// <param name="status">目标状态。</param>
    /// <param name="ct">取消令牌。</param>
    /// <exception cref="BusinessException">用户不存在时抛出。</exception>
    Task ChangeStatusAsync(long id, StatusEnum status, CancellationToken ct = default);

    /// <summary>
    /// 下载用户导入 Excel 模板（仅表头与空行）。
    /// </summary>
    /// <param name="ct">取消令牌。</param>
    Task<ExcelFileResult> GetImportTemplateAsync(CancellationToken ct = default);

    /// <summary>
    /// 按模板批量导入用户；账号已存在的行记为失败，不中断整批。
    /// </summary>
    /// <param name="stream">xlsx 文件流。</param>
    /// <param name="ct">取消令牌。</param>
    Task<ImportResultDto> ImportAsync(Stream stream, CancellationToken ct = default);

    /// <summary>
    /// 按与列表相同的筛选条件导出用户 Excel（最多 10000 行）。
    /// </summary>
    /// <param name="query">筛选条件；分页参数忽略。</param>
    /// <param name="ct">取消令牌。</param>
    Task<ExcelFileResult> ExportAsync(UserPageQuery query, CancellationToken ct = default);
}
