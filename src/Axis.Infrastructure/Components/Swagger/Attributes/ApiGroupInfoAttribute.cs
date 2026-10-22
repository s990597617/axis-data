namespace Axis.Infrastructure.Components.Swagger.Attributes;

/// <summary>
/// api 分组枚举注释
/// </summary>
public class ApiGroupInfoAttribute : Attribute
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// 版本
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// 注释
    /// </summary>
    public string Description { get; set; }
}