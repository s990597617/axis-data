namespace Axis.Scaffold.Schema;

/// <summary>
/// 参与代码生成的业务字段（已排除基类审计列）。
/// </summary>
public sealed class ScaffoldField
{
    public required string Name { get; init; }
    public required string CamelName { get; init; }
    public required string CsType { get; init; }
    public required string TsType { get; init; }
    public required string Label { get; init; }
    public int? MaxLength { get; init; }
    public bool IsNullable { get; init; }
    public bool IsRequired { get; init; }
    public string? DefaultCsLiteral { get; init; }
    public bool IsEnumStatus { get; init; }
    public bool IsString => CsType is "string" || CsType.StartsWith("string?", StringComparison.Ordinal);

    public string CsTypeDecl =>
        IsNullable && CsType is not "string" && !CsType.EndsWith('?')
            ? $"{CsType}?"
            : CsType == "string" && IsNullable
                ? "string?"
                : CsType == "string" && !IsNullable
                    ? "string"
                    : CsType;

    public string EntityCsTypeDecl
    {
        get
        {
            if (IsEnumStatus) return IsNullable ? "StatusEnum?" : "StatusEnum";
            if (CsType == "string") return IsNullable ? "string?" : "string";
            return IsNullable && !CsType.EndsWith('?') ? $"{CsType}?" : CsType;
        }
    }
}
