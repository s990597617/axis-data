namespace Axis.Infrastructure.Global;

/// <summary>
/// 全局媒体类型
/// </summary>
public static class GlobalMimeType
{
    /// <summary>
    /// 强制下载类型 - 强制浏览器下载文件而不是尝试打开
    /// </summary>
    public const string ApplicationForceDownload = "application/force-download";

    /// <summary>
    /// JSON数据格式 - 用于API响应和数据交换
    /// </summary>
    public const string ApplicationJson = "application/json";

    /// <summary>
    /// Web应用清单文件 - 用于PWA（渐进式Web应用）
    /// </summary>
    public const string ApplicationManifestJson = "application/manifest+json";

    /// <summary>
    /// 二进制流数据 - 通用二进制文件类型
    /// </summary>
    public const string ApplicationOctetStream = "application/octet-stream";

    /// <summary>
    /// PDF文档格式 - Adobe便携式文档格式
    /// </summary>
    public const string ApplicationPdf = "application/pdf";

    /// <summary>
    /// RSS订阅源 - 用于新闻订阅和博客推送
    /// </summary>
    public const string ApplicationRssXml = "application/rss+xml";

    /// <summary>
    /// XML数据格式 - 可扩展标记语言数据
    /// </summary>
    public const string ApplicationXml = "application/xml";

    /// <summary>
    /// 表单URL编码数据 - HTML表单默认提交格式
    /// </summary>
    public const string ApplicationXWwwFormUrlencoded = "application/x-www-form-urlencoded";

    /// <summary>
    /// 压缩文件格式 - ZIP压缩文件
    /// </summary>
    public const string ApplicationXZipCo = "application/x-zip-co";

    /// <summary>
    /// 标准ZIP压缩格式
    /// </summary>
    public const string ApplicationZip = "application/zip";

    /// <summary>
    /// BMP图像格式 - 位图图像文件
    /// </summary>
    public const string ImageBmp = "image/bmp";

    /// <summary>
    /// GIF图像格式 - 支持动画的位图图像格式
    /// </summary>
    public const string ImageGif = "image/gif";

    /// <summary>
    /// JPEG图像格式 - 有损压缩的照片图像
    /// </summary>
    public const string ImageJpeg = "image/jpeg";

    /// <summary>
    /// 渐进式JPEG格式 - 逐步加载的JPEG图像
    /// </summary>
    public const string ImagePJpeg = "image/pjpeg";

    /// <summary>
    /// PNG图像格式 - 支持透明度的无损压缩图像
    /// </summary>
    public const string ImagePng = "image/png";

    /// <summary>
    /// TIFF图像格式 - 高质量栅格图像格式，常用于印刷
    /// </summary>
    public const string ImageTiff = "image/tiff";

    /// <summary>
    /// CSS样式表 - 层叠样式表文件
    /// </summary>
    public const string TextCss = "text/css";

    /// <summary>
    /// CSV数据格式 - 逗号分隔值文件
    /// </summary>
    public const string TextCsv = "text/csv";

    /// <summary>
    /// JavaScript代码 - 客户端脚本文件
    /// </summary>
    public const string TextJavascript = "text/javascript";

    /// <summary>
    /// 纯文本格式 - 无格式文本文件
    /// </summary>
    public const string TextPlain = "text/plain";

    /// <summary>
    /// Excel文件格式 - Microsoft Excel Open XML格式（.xlsx）
    /// </summary>
    public const string TextXlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
}