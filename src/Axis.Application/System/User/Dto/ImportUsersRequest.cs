using Microsoft.AspNetCore.Http;

namespace Axis.Application.System.User.Dto;

/// <summary>
/// 用户 Excel 导入请求（multipart 文件）。
/// </summary>
public sealed class ImportUsersRequest
{
    /// <summary>
    /// 上传的 xlsx 文件；须与导入模板列头一致。
    /// </summary>
    public IFormFile? File { get; set; }
}
