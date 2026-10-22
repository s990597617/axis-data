namespace Axis.Scaffold.Schema;

public static class ClrTypeMapper
{
    public static (string CsType, string TsType, bool IsEnumStatus) Map(
        string rawDbType,
        string propertyName,
        bool isNullable)
    {
        var t = rawDbType.Trim().ToLowerInvariant();
        // 去掉 postgresql 精度：character varying(64) / numeric(18,2)
        var paren = t.IndexOf('(');
        if (paren >= 0) t = t[..paren];

        if (string.Equals(propertyName, "Status", StringComparison.OrdinalIgnoreCase)
            && t is "int" or "int2" or "int4" or "int8" or "integer" or "smallint" or "bigint")
        {
            return ("StatusEnum", "number", true);
        }

        var (cs, ts) = t switch
        {
            "int2" or "smallint" => ("short", "number"),
            "int4" or "int" or "integer" => ("int", "number"),
            "int8" or "bigint" => ("long", "string"),
            "float4" or "real" => ("float", "number"),
            "float8" or "double" or "double precision" => ("double", "number"),
            "numeric" or "decimal" or "money" => ("decimal", "number"),
            "bool" or "boolean" or "bit" => ("bool", "boolean"),
            "uuid" => ("Guid", "string"),
            "date" => ("DateOnly", "string"),
            "timestamp" or "timestamptz" or "timestamp without time zone"
                or "timestamp with time zone" or "datetime" or "datetime2" => ("DateTime", "string"),
            "time" or "timetz" => ("TimeSpan", "string"),
            "json" or "jsonb" or "text" or "varchar" or "character varying"
                or "character" or "char" or "nvarchar" or "nchar" or "citext" => ("string", "string"),
            _ => ("string", "string")
        };

        return (cs, ts, false);
    }

    public static (string CsType, string TsType, bool IsEnumStatus) MapFromClr(Type type, string propertyName)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        if (underlying.IsEnum
            || (string.Equals(propertyName, "Status", StringComparison.OrdinalIgnoreCase)
                && underlying == typeof(int)))
        {
            if (underlying.Name == "StatusEnum" || string.Equals(propertyName, "Status", StringComparison.OrdinalIgnoreCase))
                return ("StatusEnum", "number", true);
        }

        if (underlying == typeof(string)) return ("string", "string", false);
        if (underlying == typeof(short)) return ("short", "number", false);
        if (underlying == typeof(int)) return ("int", "number", false);
        if (underlying == typeof(long)) return ("long", "string", false);
        if (underlying == typeof(float)) return ("float", "number", false);
        if (underlying == typeof(double)) return ("double", "number", false);
        if (underlying == typeof(decimal)) return ("decimal", "number", false);
        if (underlying == typeof(bool)) return ("bool", "boolean", false);
        if (underlying == typeof(Guid)) return ("Guid", "string", false);
        if (underlying == typeof(DateTime)) return ("DateTime", "string", false);
        if (underlying == typeof(DateOnly)) return ("DateOnly", "string", false);
        if (underlying == typeof(TimeSpan)) return ("TimeSpan", "string", false);
        if (underlying.IsEnum) return (underlying.Name, "number", underlying.Name == "StatusEnum");

        return ("string", "string", false);
    }
}
