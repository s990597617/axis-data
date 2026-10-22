namespace Axis.Application.System.File.Dto;

/// <summary>
/// 按 FileId 操作的路径参数。
/// </summary>
public sealed class FileIdRequest
{
    /// <summary>
    /// 文件业务标识（雪花字符串）。
    /// </summary>
    public string FileId { get; set; } = null!;
}
