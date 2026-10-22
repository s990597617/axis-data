namespace Axis.Infrastructure.Options;

/// <summary>
/// 代码生成配置（管理端网页 + CLI 共用约定）。
/// </summary>
/// <remarks>
/// 模块脚手架引擎见 <c>tools/Axis.Scaffold.Core</c>；用法见 <c>docs/脚手架.md</c>。
/// 生产环境请保持 <see cref="Enabled"/> = false。
/// </remarks>
public class AxisCodeGenOptions
{
    public const string SectionName = "CodeGen";

    /// <summary>
    /// 是否启用管理端代码生成 API（默认关闭）。
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 是否允许在生产环境启用（默认 false，即使 Enabled=true 也会拒绝）。
    /// </summary>
    public bool AllowInProduction { get; set; }

    /// <summary>
    /// 仓库根路径（含 Axis.Net.sln）。为空则从 ContentRoot 向上查找。
    /// </summary>
    public string RepoRootPath { get; set; }

    /// <summary>
    /// 数据库实体程序集名称集合。
    /// </summary>
    public List<string> EntityAssemblyNames { get; set; }

    /// <summary>
    /// 数据库基础实体名称集合。
    /// </summary>
    public List<string> BaseEntityNames { get; set; }

    /// <summary>
    /// 基础实体与列名映射。
    /// </summary>
    public Dictionary<string, string[]> BaseEntityColumn { get; set; }

    /// <summary>
    /// 前端文件根目录（相对仓库根或绝对路径；脚手架默认使用 frontend/）。
    /// </summary>
    public string FrontRootPath { get; set; }

    /// <summary>
    /// 后端生成目标项目命名空间列表。
    /// </summary>
    public List<string> BackendApplicationNamespaces { get; set; }
}
