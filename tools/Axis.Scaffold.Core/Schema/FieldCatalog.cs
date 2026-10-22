namespace Axis.Scaffold.Schema;

public static class FieldCatalog
{
    /// <summary>
    /// 基类 / 框架列，生成业务字段时跳过（含下划线形式）。
    /// </summary>
    public static readonly HashSet<string> SkipNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id", "CreatedBy", "Creator", "CreatedTime",
        "ModifiedBy", "Modifier", "ModifiedTime",
        "Deleted", "Version", "TenantId",
        "created_by", "creator", "created_time",
        "modified_by", "modifier", "modified_time",
        "deleted", "version", "tenant_id", "id"
    };

    /// <summary>
    /// 一期默认五字段（无 --from-table / --from-entity 时使用）。
    /// </summary>
    public static IReadOnlyList<ScaffoldField> DefaultFields(string cnName) =>
    [
        new ScaffoldField
        {
            Name = "Name",
            CamelName = "name",
            CsType = "string",
            TsType = "string",
            Label = $"{cnName}名称",
            MaxLength = 64,
            IsNullable = false,
            IsRequired = true
        },
        new ScaffoldField
        {
            Name = "Code",
            CamelName = "code",
            CsType = "string",
            TsType = "string",
            Label = $"{cnName}编码",
            MaxLength = 64,
            IsNullable = true,
            IsRequired = false
        },
        new ScaffoldField
        {
            Name = "OrderNo",
            CamelName = "orderNo",
            CsType = "int",
            TsType = "number",
            Label = "排序",
            IsNullable = false,
            IsRequired = true,
            DefaultCsLiteral = "100"
        },
        new ScaffoldField
        {
            Name = "Status",
            CamelName = "status",
            CsType = "StatusEnum",
            TsType = "number",
            Label = "状态",
            IsNullable = false,
            IsRequired = true,
            DefaultCsLiteral = "StatusEnum.Enable",
            IsEnumStatus = true
        },
        new ScaffoldField
        {
            Name = "Remark",
            CamelName = "remark",
            CsType = "string",
            TsType = "string",
            Label = "备注",
            MaxLength = 256,
            IsNullable = true,
            IsRequired = false
        }
    ];

    public static string ToPascal(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return name;
        if (!name.Contains('_') && char.IsUpper(name[0])) return name;

        var parts = name.Split('_', StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(parts.Select(static p =>
            char.ToUpperInvariant(p[0]) + p[1..].ToLowerInvariant()));
    }

    public static string ToCamel(string pascal)
        => string.IsNullOrEmpty(pascal)
            ? pascal
            : char.ToLowerInvariant(pascal[0]) + pascal[1..];
}
