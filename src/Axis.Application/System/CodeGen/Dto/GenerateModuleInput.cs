namespace Axis.Application.System.CodeGen.Dto;

/// <summary>
/// 生成标准 CRUD 模块入参（对齐 CLI new module）。
/// </summary>
public sealed class GenerateModuleInput
{
    /// <summary>PascalCase 模块名，如 Product。</summary>
    public string Module { get; set; } = "";

    /// <summary>中文名。</summary>
    public string CnName { get; set; } = "";

    /// <summary>业务域，默认 Demo。</summary>
    public string Area { get; set; } = "Demo";

    /// <summary>URL / 权限段（kebab），可空自动推导。</summary>
    public string? RouteKey { get; set; }

    /// <summary>实体名，可空。</summary>
    public string? Entity { get; set; }

    /// <summary>表名，可空。</summary>
    public string? Table { get; set; }

    /// <summary>从数据库表列生成字段。</summary>
    public string? FromTable { get; set; }

    /// <summary>从 Domain 实体反射字段。</summary>
    public string? FromEntity { get; set; }

    public bool Force { get; set; }
    public bool BackendOnly { get; set; }
    public bool FrontendOnly { get; set; }
    public bool NoMenu { get; set; }
}
