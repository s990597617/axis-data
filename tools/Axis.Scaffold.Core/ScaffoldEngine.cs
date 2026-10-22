using Axis.Scaffold.Schema;

namespace Axis.Scaffold;

public sealed class ScaffoldEngine
{
    private readonly string _repoRoot;
    private readonly string _templateRoot;

    public ScaffoldEngine(string repoRoot)
    {
        _repoRoot = repoRoot;
        _templateRoot = Path.Combine(repoRoot, "templates", "crud-module");
        if (!Directory.Exists(_templateRoot))
            throw new DirectoryNotFoundException($"????????{_templateRoot}");
    }

    public static string FindRepoRoot(string startDirectory)
    {
        var dir = new DirectoryInfo(startDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Axis.Net.sln")))
                return dir.FullName;
            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException(
            "???????? Axis.Net.sln????????????????????");
    }

    public ScaffoldResult Generate(NewModuleOptions options, SqlSugar.ISqlSugarClient? db = null)
    {
        if (!string.IsNullOrWhiteSpace(options.FromTable) && !string.IsNullOrWhiteSpace(options.FromEntity))
            throw new ArgumentException("?????? --from-table ? --from-entity?");

        var tableOverride = options.Table;
        string fieldSource;
        IReadOnlyList<ScaffoldField> fields;

        if (!string.IsNullOrWhiteSpace(options.FromTable))
        {
            tableOverride = options.FromTable.Trim();
            fields = db is not null
                ? TableSchemaReader.Read(db, tableOverride)
                : TableSchemaReader.Read(_repoRoot, tableOverride);
            fieldSource = $"table:{tableOverride}";
        }
        else if (!string.IsNullOrWhiteSpace(options.FromEntity))
        {
            var (entityFields, sugarTable) = EntitySchemaReader.Read(_repoRoot, options.FromEntity.Trim());
            fields = entityFields;
            fieldSource = $"entity:{options.FromEntity.Trim()}";
            if (string.IsNullOrWhiteSpace(tableOverride) && !string.IsNullOrWhiteSpace(sugarTable))
                tableOverride = sugarTable;
        }
        else
        {
            fields = FieldCatalog.DefaultFields(options.CnName);
            fieldSource = "default";
        }

        var tokens = TokenContext.Create(
            options.Module,
            options.CnName,
            options.Area,
            options.RouteKey,
            options.Entity,
            tableOverride);

        var fieldRenderer = new FieldSnippetRenderer(fields, tokens.CnName, tokens.Entity)
        {
            SourceLabel = fieldSource
        };
        var manifest = ScaffoldManifest.Load(Path.Combine(_templateRoot, "manifest.json"));
        var flags = ResolveFlags(tokens);
        var actions = new List<FileAction>();

        foreach (var entry in manifest.Files)
        {
            var side = string.IsNullOrWhiteSpace(entry.Side) ? "backend" : entry.Side;
            if (options.BackendOnly && side == "frontend") continue;
            if (options.FrontendOnly && side == "backend") continue;
            if (!ShouldInclude(entry.When, flags)) continue;

            var relativeTo = tokens.Apply(entry.To).Replace('/', Path.DirectorySeparatorChar);
            if (relativeTo.Contains(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                || relativeTo.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                throw new InvalidOperationException($"???????{relativeTo}");

            var source = Path.Combine(_templateRoot, entry.From.Replace('/', Path.DirectorySeparatorChar));
            var target = Path.GetFullPath(Path.Combine(_repoRoot, relativeTo));
            if (!target.StartsWith(_repoRoot, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"???????{target}");

            if (!File.Exists(source))
                throw new FileNotFoundException($"????????{source}");

            var exists = File.Exists(target);
            if (exists && !options.Force)
            {
                actions.Add(new FileAction(target, "SKIP", "?????? --force ???", Content: null));
                continue;
            }

            var content = tokens.Apply(fieldRenderer.Apply(File.ReadAllText(source)));
            if (options.DryRun)
            {
                actions.Add(new FileAction(
                    target,
                    exists ? "OVERWRITE" : "CREATE",
                    "dry-run",
                    Content: content));
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            File.WriteAllText(target, content);
            actions.Add(new FileAction(target, exists ? "OVERWRITE" : "CREATE", Detail: null, Content: content));
        }

        // ????????????????????? snippet?
        if (!options.FrontendOnly && flags.AreaPermissionMerge)
            actions.Add(PermissionMerger.MergeBackend(_repoRoot, tokens, options.DryRun, options.Force));

        if (!options.BackendOnly && flags.FrontendPermissionMerge)
            actions.Add(PermissionMerger.MergeFrontend(_repoRoot, tokens, options.DryRun, options.Force));

        // ???????????EnableInitSeed ????--no-menu ????
        if (!options.FrontendOnly && !options.NoMenu)
            actions.Add(MenuSeedWriter.Write(_repoRoot, tokens, options.DryRun, options.Force));

        var notes = BuildChecklist(tokens, flags, options, fieldSource, fields).ToList();
        return new ScaffoldResult(tokens, actions, notes, fields, fieldSource);
    }

    private ConditionFlags ResolveFlags(TokenContext tokens)
    {
        var areaPermissionPath = Path.Combine(
            _repoRoot,
            "src",
            "Axis.Domain",
            tokens.Area,
            $"{tokens.PermissionCodesType}.cs");

        var areaPermissionExists = File.Exists(areaPermissionPath);
        var areaPermissionFull = !tokens.IsSystemArea && !areaPermissionExists;
        var areaPermissionMerge = areaPermissionExists; // System ?????? ? ??

        var frontendCodesPath = Path.Combine(
            _repoRoot,
            "frontend",
            "src",
            "modules",
            tokens.AreaLower,
            "permissions",
            "codes.ts");
        var frontendCodesExists = File.Exists(frontendCodesPath);

        return new ConditionFlags(
            areaPermissionFull,
            areaPermissionMerge,
            !frontendCodesExists,
            frontendCodesExists);
    }

    private static bool ShouldInclude(string? when, ConditionFlags flags)
        => when switch
        {
            null or "" => true,
            "areaPermissionFull" => flags.AreaPermissionFull,
            "frontendPermissionFull" => flags.FrontendPermissionFull,
            // ? snippet when ??????
            "areaPermissionSnippet" or "frontendPermissionSnippet" => false,
            _ => throw new InvalidOperationException($"?? when ???{when}")
        };

    private static IEnumerable<string> BuildChecklist(
        TokenContext tokens,
        ConditionFlags flags,
        NewModuleOptions options,
        string fieldSource,
        IReadOnlyList<ScaffoldField> fields)
    {
        yield return "?? ??????? ??";
        yield return $"0. ?????{fieldSource}?? {fields.Count} ??????{string.Join(", ", fields.Select(f => f.Name))}?";
        yield return "1. ?????dotnet build Axis.Net.sln";
        if (!options.BackendOnly)
            yield return "2. ???? pnpm ???? / ???";
        yield return $"3. ?? API ???{tokens.RoutePrefix}??????{tokens.PermPrefix}";
        yield return $"4. ?? component?modules/{tokens.AreaLower}/views/{tokens.ModuleKebab}/index";

        if (!options.FrontendOnly && flags.AreaPermissionFull)
            yield return $"5. ??? {tokens.PermissionCodesType}.cs?";
        if (!options.FrontendOnly && flags.AreaPermissionMerge)
            yield return $"5. ???????? {tokens.PermissionCodesType}?";
        if (!options.BackendOnly && flags.FrontendPermissionFull)
            yield return "6. ????? permissions/codes.ts?";
        if (!options.BackendOnly && flags.FrontendPermissionMerge)
            yield return $"6. ?????????? {tokens.ModuleCamel}?";

        if (!options.FrontendOnly && !options.NoMenu)
        {
            yield return
                $"7. ??????? System/Seeds/Generated/{tokens.Area}_{tokens.Module}MenuSeed.cs?? EnableInitSeed ???????????";
        }
        else
        {
            yield return "7. ????????--no-menu??";
        }

        yield return "8. ???? ApiGroupEnum / RoleMenu??? Swagger ????? AdminBaseModule?";
    }

    private sealed record ConditionFlags(
        bool AreaPermissionFull,
        bool AreaPermissionMerge,
        bool FrontendPermissionFull,
        bool FrontendPermissionMerge);
}

public sealed record NewModuleOptions(
    string Module,
    string CnName,
    string Area,
    string? RouteKey,
    string? Entity,
    string? Table,
    string? FromTable,
    string? FromEntity,
    bool DryRun,
    bool Force,
    bool BackendOnly,
    bool FrontendOnly,
    bool NoMenu);

public sealed record FileAction(string Path, string Action, string? Detail, string? Content = null);

public sealed record ScaffoldResult(
    TokenContext Tokens,
    IReadOnlyList<FileAction> Actions,
    IReadOnlyList<string> Notes,
    IReadOnlyList<ScaffoldField> Fields,
    string FieldSource);
