using System.Text.Json;
using SqlSugar;

namespace Axis.Scaffold.Schema;

public static class TableSchemaReader
{
    public static IReadOnlyList<ScaffoldField> Read(string repoRoot, string tableName)
    {
        var (dbType, connectionString) = LoadMasterConnection(repoRoot);
        using var db = new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = dbType,
            IsAutoCloseConnection = true
        });
        return Read(db, tableName);
    }

    /// <summary>
    /// 使用已注入的 SqlSugar 客户端读表列（管理端网页生成优先走此重载）。
    /// </summary>
    public static IReadOnlyList<ScaffoldField> Read(ISqlSugarClient db, string tableName)
    {
        ArgumentNullException.ThrowIfNull(db);
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("表名不能为空。", nameof(tableName));

        if (!db.DbMaintenance.IsAnyTable(tableName, false))
            throw new InvalidOperationException($"数据库中不存在表：{tableName}");

        var columns = db.DbMaintenance.GetColumnInfosByTableName(tableName, false);
        var fields = MapColumns(columns);

        if (fields.Count == 0)
            throw new InvalidOperationException($"表 {tableName} 没有可生成的业务列（均被基类列过滤）。");

        return fields;
    }

    /// <summary>
    /// 列出主库用户表。
    /// </summary>
    public static IReadOnlyList<string> ListTables(ISqlSugarClient db)
    {
        ArgumentNullException.ThrowIfNull(db);
        var tables = db.DbMaintenance.GetTableInfoList(false) ?? [];
        return tables
            .Select(t => t.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static List<ScaffoldField> MapColumns(IEnumerable<DbColumnInfo> columns)
    {
        var fields = new List<ScaffoldField>();

        foreach (var col in columns)
        {
            var rawName = col.DbColumnName ?? col.PropertyName ?? "";
            if (string.IsNullOrWhiteSpace(rawName)) continue;
            if (FieldCatalog.SkipNames.Contains(rawName)) continue;

            var pascal = FieldCatalog.ToPascal(rawName);
            if (FieldCatalog.SkipNames.Contains(pascal)) continue;

            var (cs, ts, isStatus) = ClrTypeMapper.Map(col.DataType ?? "varchar", pascal, col.IsNullable);
            var maxLen = col.Length is > 0 and <= 8000 ? (int?)col.Length : null;
            if (cs != "string") maxLen = null;

            var label = string.IsNullOrWhiteSpace(col.ColumnDescription) ? pascal : col.ColumnDescription.Trim();
            var isRequired = !col.IsNullable && cs == "string";

            fields.Add(new ScaffoldField
            {
                Name = pascal,
                CamelName = FieldCatalog.ToCamel(pascal),
                CsType = isStatus ? "StatusEnum" : cs,
                TsType = ts,
                Label = label,
                MaxLength = maxLen,
                IsNullable = col.IsNullable,
                IsRequired = isRequired || (!col.IsNullable && cs != "string" && !isStatus),
                DefaultCsLiteral = GuessDefault(pascal, cs, isStatus, col.IsNullable),
                IsEnumStatus = isStatus
            });
        }

        return fields;
    }

    private static string? GuessDefault(string name, string cs, bool isStatus, bool nullable)
    {
        if (nullable) return null;
        if (isStatus) return "StatusEnum.Enable";
        if (string.Equals(name, "OrderNo", StringComparison.OrdinalIgnoreCase)) return "100";
        return cs switch
        {
            "int" or "short" or "long" => "0",
            "bool" => "false",
            "decimal" or "float" or "double" => "0",
            _ => null
        };
    }

    private static (DbType DbType, string ConnectionString) LoadMasterConnection(string repoRoot)
    {
        var path = Path.Combine(repoRoot, "src", "Axis.Host.Api", "Config", "08-Db.json");
        if (!File.Exists(path))
            throw new FileNotFoundException($"未找到数据库配置：{path}");

        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        if (!doc.RootElement.TryGetProperty("DbConnection", out var dbRoot)
            || !dbRoot.TryGetProperty("ConnectionConfigs", out var configs)
            || configs.GetArrayLength() == 0)
            throw new InvalidOperationException("08-Db.json 缺少 DbConnection.ConnectionConfigs。");

        var master = configs.EnumerateArray().FirstOrDefault(c =>
            c.TryGetProperty("ConfigId", out var id)
            && string.Equals(id.GetString(), "master", StringComparison.OrdinalIgnoreCase));

        if (master.ValueKind == JsonValueKind.Undefined)
            master = configs[0];

        var cs = master.GetProperty("ConnectionString").GetString()
                 ?? throw new InvalidOperationException("连接串为空。");
        var dbTypeName = master.TryGetProperty("DbType", out var dt) ? dt.GetString() : "PostgreSQL";
        if (!Enum.TryParse<DbType>(dbTypeName, ignoreCase: true, out var dbType))
            dbType = DbType.PostgreSQL;

        return (dbType, cs);
    }
}
