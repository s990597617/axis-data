using System.Text;

namespace Axis.Scaffold.Schema;

/// <summary>
/// 将字段列表渲染为模板中的 __FIELD_*__ 令牌。
/// </summary>
public sealed class FieldSnippetRenderer
{
    private readonly IReadOnlyList<ScaffoldField> _fields;
    private readonly string _cnName;
    private readonly string _entity;
    private readonly Dictionary<string, string> _tokens;

    public FieldSnippetRenderer(IReadOnlyList<ScaffoldField> fields, string cnName, string entity)
    {
        _fields = fields;
        _cnName = cnName;
        _entity = entity;
        _tokens = Build();
    }

    public string SourceLabel { get; init; } = "default";

    public IReadOnlyList<ScaffoldField> Fields => _fields;

    public string Apply(string input)
    {
        var result = input;
        foreach (var (key, value) in _tokens.OrderByDescending(kv => kv.Key.Length))
            result = result.Replace(key, value, StringComparison.Ordinal);
        return result;
    }

    private Dictionary<string, string> Build()
    {
        var hasCode = Has("Code");
        var hasStatus = _fields.Any(f => f.IsEnumStatus || f.Name == "Status");
        var stringFields = _fields.Where(f => f.IsString).ToList();
        var keywordTargets = stringFields
            .Where(f => f.Name is "Name" or "Code")
            .DefaultIfEmpty(stringFields.FirstOrDefault()!)
            .Where(f => f is not null)
            .DistinctBy(f => f.Name)
            .ToList();

        return new Dictionary<string, string>
        {
            ["__FIELD_ENTITY_USINGS__"] = hasStatus || _fields.Any(f => f.CsType == "StatusEnum")
                ? "using Axis.Domain.Enums;\r\n"
                : "",
            ["__FIELD_ENTITY_INDEX__"] = hasCode
                ? "[SugarIndex(\"index_{table}_code\", nameof(Code), OrderByType.Asc)]\r\n"
                : "",
            ["__FIELD_ENTITY_PROPS__"] = RenderEntityProps(),
            ["__FIELD_DTO_USINGS__"] = NeedsEnumUsing() ? "using Axis.Domain.Enums;\r\n" : "",
            ["__FIELD_DTO_PROPS__"] = RenderDtoProps(includeAudit: true),
            ["__FIELD_SAVE_PROPS__"] = RenderSaveProps(),
            ["__FIELD_PAGE_QUERY_USINGS__"] = hasStatus ? "using Axis.Domain.Enums;\r\n" : "",
            ["__FIELD_PAGE_QUERY_EXTRA__"] = hasStatus
                ? """

    /// <summary>
    /// 启用状态；为空表示不按状态过滤。
    /// </summary>
    public StatusEnum? Status { get; set; }

"""
                : "\r\n",
            ["__FIELD_VALIDATOR_BODY__"] = RenderValidatorBody(),
            ["__FIELD_SELECT_PROPS__"] = RenderSelectProps(),
            ["__FIELD_CREATE_ASSIGNS__"] = RenderCreateAssigns(),
            ["__FIELD_UPDATE_ASSIGNS__"] = RenderUpdateAssigns(),
            ["__FIELD_KEYWORD_WHERE__"] = RenderKeywordWhere(keywordTargets),
            ["__FIELD_STATUS_WHERE__"] = hasStatus
                ? ".WhereIF(query.Status.HasValue, item => item.Status == query.Status!.Value)"
                : "",
            ["__FIELD_CODE_UNIQUE_CREATE__"] = hasCode
                ? "        await EnsureCodeUniqueAsync(input.Code, null, ct);\r\n"
                : "",
            ["__FIELD_CODE_UNIQUE_UPDATE__"] = hasCode
                ? "        await EnsureCodeUniqueAsync(input.Code, id, ct);\r\n"
                : "",
            ["__FIELD_CODE_UNIQUE_METHOD__"] = hasCode ? RenderCodeUniqueMethod() : "",
            ["__FIELD_FE_DTO__"] = RenderFeDto(),
            ["__FIELD_FE_SAVE__"] = RenderFeSave(),
            ["__FIELD_FE_COLUMNS__"] = RenderFeColumns(hasStatus),
            ["__FIELD_FE_FORM_FIELDS__"] = RenderFeFormFields(hasStatus),
            ["__FIELD_FE_STATUS_IMPORT__"] = hasStatus
                ? "  renderStatus,\r\n  statusOptions,\r\n"
                : "",
            ["__FIELD_SOURCE__"] = SourceLabel
        };
    }

    private bool Has(string name) => _fields.Any(f => f.Name == name);
    private bool NeedsEnumUsing() => _fields.Any(f => f.IsEnumStatus || f.CsType == "StatusEnum");

    private string RenderEntityProps()
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            """
    /// <summary>
    /// 租户 Id。
    /// </summary>
    [SugarColumn(ColumnDescription = "租户Id")]
    public long? TenantId { get; set; }
""");

        foreach (var f in _fields)
        {
            sb.AppendLine(
                $$"""
    /// <summary>
    /// {{f.Label}}。
    /// </summary>
""");
            if (f.IsRequired && f.CsType == "string")
                sb.AppendLine("    [Required]");
            if (f.MaxLength is > 0)
                sb.AppendLine($"    [MaxLength({f.MaxLength})]");

            var sugarParts = new List<string> { $"ColumnDescription = \"{Escape(f.Label)}\"" };
            if (f.MaxLength is > 0) sugarParts.Add($"Length = {f.MaxLength}");
            if (f.IsNullable) sugarParts.Add("IsNullable = true");
            sb.AppendLine($"    [SugarColumn({string.Join(", ", sugarParts)})]");

            var def = f.DefaultCsLiteral is null ? "" : $" = {f.DefaultCsLiteral}";
            var bang = !f.IsNullable && f.CsType == "string" && f.DefaultCsLiteral is null ? " = null!" : "";
            sb.AppendLine($"    public {f.EntityCsTypeDecl} {f.Name} {{ get; set; }}{def}{bang};");
            sb.AppendLine();
        }

        return sb.ToString().TrimEnd() + "\r\n";
    }

    private string RenderDtoProps(bool includeAudit)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            """
    /// <summary>
    /// 主键 Id。
    /// </summary>
    public long Id { get; set; }
""");

        foreach (var f in _fields)
        {
            sb.AppendLine(
                $$"""

    /// <summary>
    /// {{f.Label}}。
    /// </summary>
    public {{DtoType(f)}} {{f.Name}} { get; set; }{{DtoInit(f)}};
""");
        }

        if (includeAudit)
        {
            sb.AppendLine(
                """

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>
    /// 最后修改时间；从未修改时为空。
    /// </summary>
    public DateTime? ModifiedTime { get; set; }
""");
        }

        return sb.ToString().TrimEnd() + "\r\n";
    }

    private string RenderSaveProps()
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            """
    /// <summary>
    /// 主键；更新时必填，新增时可忽略。
    /// </summary>
    public long Id { get; set; }
""");

        foreach (var f in _fields)
        {
            sb.AppendLine();
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// {f.Label}。");
            sb.AppendLine($"    /// </summary>");
            if (f.IsRequired && f.CsType == "string")
                sb.AppendLine("    [Required, MaxLength(" + (f.MaxLength ?? 64) + ")]");
            else if (f.MaxLength is > 0)
                sb.AppendLine($"    [MaxLength({f.MaxLength})]");

            var def = f.DefaultCsLiteral is null ? "" : $" = {f.DefaultCsLiteral}";
            var bang = !f.IsNullable && f.CsType == "string" && f.DefaultCsLiteral is null ? " = null!" : "";
            sb.AppendLine($"    public {SaveType(f)} {f.Name} {{ get; set; }}{def}{bang};");
        }

        return sb.ToString().TrimEnd() + "\r\n";
    }

    private string RenderValidatorBody()
    {
        var sb = new StringBuilder();
        foreach (var f in _fields.Where(x => x.CsType == "string" || x.IsString))
        {
            if (f.IsRequired)
            {
                sb.AppendLine(
                    $$"""
        RuleFor(x => x.{{f.Name}})
            .NotEmpty().WithMessage("{{f.Label}}不能为空")
            .MaximumLength({{f.MaxLength ?? 64}}).WithMessage("{{f.Label}}不能超过 {{f.MaxLength ?? 64}} 个字符");
""");
            }
            else if (f.MaxLength is > 0)
            {
                sb.AppendLine(
                    $$"""
        RuleFor(x => x.{{f.Name}})
            .MaximumLength({{f.MaxLength}}).WithMessage("{{f.Label}}不能超过 {{f.MaxLength}} 个字符")
            .When(x => x.{{f.Name}} is not null);
""");
            }
        }

        return sb.ToString().TrimEnd() + "\r\n";
    }

    private string RenderSelectProps()
    {
        var sb = new StringBuilder();
        sb.AppendLine("                Id = item.Id,");
        foreach (var f in _fields)
            sb.AppendLine($"                {f.Name} = item.{f.Name},");
        sb.AppendLine("                CreatedTime = item.CreatedTime,");
        sb.Append("                ModifiedTime = item.ModifiedTime");
        return sb.ToString();
    }

    private string RenderCreateAssigns()
    {
        var sb = new StringBuilder();
        foreach (var f in _fields)
            sb.AppendLine($"            {f.Name} = input.{f.Name},");
        return sb.ToString().TrimEnd();
    }

    private string RenderUpdateAssigns()
    {
        var sb = new StringBuilder();
        foreach (var f in _fields)
            sb.AppendLine($"        entity.{f.Name} = input.{f.Name};");
        return sb.ToString().TrimEnd();
    }

    private string RenderKeywordWhere(List<ScaffoldField> targets)
    {
        if (targets.Count == 0)
            return ".WhereIF(false, item => true)";

        var parts = targets.Select(f =>
            f.IsNullable || f.CsType == "string"
                ? $"(item.{f.Name} != null && item.{f.Name}.Contains(query.Keyword!))"
                : $"item.{f.Name}.Contains(query.Keyword!)");

        // Name 非空 string 常用 Contains 直接写
        parts = targets.Select(f =>
        {
            if (f.Name == "Name" && !f.IsNullable)
                return $"item.{f.Name}.Contains(query.Keyword!)";
            if (f.IsNullable || f.Name == "Code")
                return $"(item.{f.Name} != null && item.{f.Name}.Contains(query.Keyword!))";
            return $"item.{f.Name}.Contains(query.Keyword!)";
        });

        return $".WhereIF(!string.IsNullOrWhiteSpace(query.Keyword), item => {string.Join(" || ", parts)})";
    }

    private string RenderCodeUniqueMethod() =>
        $$"""

    /// <summary>
    /// 校验非空编码在当前租户内唯一。
    /// </summary>
    private async Task EnsureCodeUniqueAsync(string? code, long? excludedId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code)) return;
        var exists = await Repo.AsQueryable()
            .Where(item => item.Code == code)
            .WhereIF(CurrentAccount.TenantId.HasValue, item => item.TenantId == CurrentAccount.TenantId)
            .WhereIF(excludedId.HasValue, item => item.Id != excludedId!.Value)
            .AnyAsync(ct);
        if (exists) throw new BusinessException($"{{_cnName}}编码 '{code}' 已存在");
    }

""";

    private string RenderFeDto()
    {
        var sb = new StringBuilder();
        sb.AppendLine("  id: string");
        foreach (var f in _fields)
        {
            var opt = f.IsNullable || !f.IsRequired ? "?" : "";
            sb.AppendLine($"  {f.CamelName}{opt}: {f.TsType}");
        }

        sb.Append("  createdTime: string");
        return sb.ToString();
    }

    private string RenderFeSave()
    {
        var sb = new StringBuilder();
        foreach (var f in _fields)
        {
            var opt = f.IsNullable || !f.IsRequired ? "?" : "";
            sb.AppendLine($"  {f.CamelName}{opt}: {f.TsType}");
        }

        return sb.ToString().TrimEnd();
    }

    private string RenderFeColumns(bool hasStatus)
    {
        var sb = new StringBuilder();
        var firstString = _fields.FirstOrDefault(f => f.IsString);
        foreach (var f in _fields)
        {
            if (f.IsEnumStatus || f.Name == "Status")
            {
                sb.AppendLine(
                    """
    {
      key: 'status',
      title: '状态',
      dict: statusOptions,
      search: true,
      render: row => renderStatus(row.status)
    },
""");
                continue;
            }

            if (f.Name == "CreatedTime") continue;

            var search = firstString is not null && f.Name == firstString.Name
                ? $", search: {{ field: 'keyword', placeholder: '关键字' }}"
                : "";
            var width = f.Name is "OrderNo" ? ", width: 80" : "";
            var ellipsis = f.Name is "Remark" || (f.MaxLength is > 100)
                ? ", ellipsis: { tooltip: true }"
                : "";

            if (f.CsType == "DateTime")
            {
                sb.AppendLine(
                    $"    {{ key: '{f.CamelName}', title: '{Escape(f.Label)}', render: row => formatDateTime(row.{f.CamelName}) }},");
                continue;
            }

            sb.AppendLine(
                $"    {{ key: '{f.CamelName}', title: '{Escape(f.Label)}'{search}{width}{ellipsis} }},");
        }

        sb.Append(
            "    { key: 'createdTime', title: '创建时间', render: row => formatDateTime(row.createdTime) }");
        return sb.ToString();
    }

    private string RenderFeFormFields(bool hasStatus)
    {
        var sb = new StringBuilder();
        foreach (var f in _fields)
        {
            if (f.IsEnumStatus || f.Name == "Status")
            {
                sb.AppendLine(
                    """
          {
            field: 'status',
            label: '状态',
            component: 'radio',
            options: statusOptions,
            required: true,
            defaultValue: 1
          },
""");
                continue;
            }

            if (f.CsType is "int" or "short" or "long" or "decimal" or "float" or "double")
            {
                var def = f.DefaultCsLiteral ?? "0";
                if (def == "100") def = "100";
                sb.AppendLine(
                    $$"""
          {
            field: '{{f.CamelName}}',
            label: '{{Escape(f.Label)}}',
            component: 'inputNumber',
            required: {{(f.IsRequired || !f.IsNullable).ToString().ToLowerInvariant()}},
            defaultValue: {{def}},
            componentProps: { min: 0, max: 999999, placeholder: '请输入{{Escape(f.Label)}}' }
          },
""");
                continue;
            }

            if (f.CsType == "bool")
            {
                sb.AppendLine(
                    $$"""
          {
            field: '{{f.CamelName}}',
            label: '{{Escape(f.Label)}}',
            component: 'switch',
            defaultValue: false
          },
""");
                continue;
            }

            if (f.Name == "Remark" || (f.MaxLength is > 120))
            {
                sb.AppendLine(
                    $$"""
          {
            field: '{{f.CamelName}}',
            label: '{{Escape(f.Label)}}',
            component: 'textarea',
            fullWidth: true,
            defaultValue: '',
            componentProps: {
              maxlength: {{f.MaxLength ?? 256}},
              showCount: true,
              rows: 2,
              placeholder: '请输入{{Escape(f.Label)}}'
            }
          },
""");
                continue;
            }

            sb.AppendLine(
                $$"""
          {
            field: '{{f.CamelName}}',
            label: '{{Escape(f.Label)}}',
            required: {{f.IsRequired.ToString().ToLowerInvariant()}},
            defaultValue: '',
            componentProps: { maxlength: {{f.MaxLength ?? 64}}, placeholder: '请输入{{Escape(f.Label)}}' }
          },
""");
        }

        return sb.ToString().TrimEnd();
    }

    private static string DtoType(ScaffoldField f)
    {
        if (f.IsEnumStatus) return "StatusEnum";
        if (f.CsType == "string") return f.IsNullable ? "string?" : "string";
        return f.IsNullable && !f.CsType.EndsWith('?') ? $"{f.CsType}?" : f.CsType;
    }

    private static string DtoInit(ScaffoldField f)
        => !f.IsNullable && f.CsType == "string" ? " = null!" : "";

    private static string SaveType(ScaffoldField f)
    {
        if (f.IsEnumStatus) return "StatusEnum";
        if (f.CsType == "string") return f.IsNullable || !f.IsRequired ? "string?" : "string";
        return f.IsNullable ? $"{f.CsType}?" : f.CsType;
    }

    private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
