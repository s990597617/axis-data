using System.CommandLine;
using Axis.Scaffold;

var moduleArg = new Argument<string>("module")
{
    Description = "模块名（PascalCase），如 Product"
};

var cnOption = new Option<string>("--cn")
{
    Description = "中文名（Swagger / 权限 / UI）",
    Required = true
};
var areaOption = new Option<string>("--area")
{
    Description = "业务域（PascalCase），如 Demo / Biz / System",
    DefaultValueFactory = _ => "Demo"
};
var routeKeyOption = new Option<string?>("--route-key")
{
    Description = "URL / 权限段（kebab），默认由模块名推导"
};
var entityOption = new Option<string?>("--entity")
{
    Description = "实体名，默认 Sys{Module}；与 --from-entity 同时时可省略"
};
var tableOption = new Option<string?>("--table")
{
    Description = "表名，默认 {sys|area}_{route_key}；指定 --from-table 时以表名为准"
};
var fromTableOption = new Option<string?>("--from-table")
{
    Description = "从数据库表列生成业务字段（读 src/Axis.Host.Api/Config/08-Db.json）"
};
var fromEntityOption = new Option<string?>("--from-entity")
{
    Description = "从已编译 Axis.Domain 实体反射生成业务字段（需先 build Domain）"
};
var dryRunOption = new Option<bool>("--dry-run")
{
    Description = "只打印将写入的路径，不落盘"
};
var forceOption = new Option<bool>("--force")
{
    Description = "覆盖已存在文件"
};
var backendOnlyOption = new Option<bool>("--backend-only")
{
    Description = "仅生成后端"
};
var frontendOnlyOption = new Option<bool>("--frontend-only")
{
    Description = "仅生成前端"
};
var noMenuOption = new Option<bool>("--no-menu")
{
    Description = "不生成菜单种子类"
};

var moduleCommand = new Command("module", "生成标准 CRUD 模块（可按表/实体列生成字段）")
{
    moduleArg,
    cnOption,
    areaOption,
    routeKeyOption,
    entityOption,
    tableOption,
    fromTableOption,
    fromEntityOption,
    dryRunOption,
    forceOption,
    backendOnlyOption,
    frontendOnlyOption,
    noMenuOption
};

moduleCommand.SetAction(parseResult =>
{
    try
    {
        var backendOnly = parseResult.GetValue(backendOnlyOption);
        var frontendOnly = parseResult.GetValue(frontendOnlyOption);
        if (backendOnly && frontendOnly)
        {
            Console.Error.WriteLine("不能同时指定 --backend-only 与 --frontend-only。");
            return 1;
        }

        var fromTable = parseResult.GetValue(fromTableOption);
        var fromEntity = parseResult.GetValue(fromEntityOption);
        if (!string.IsNullOrWhiteSpace(fromTable) && !string.IsNullOrWhiteSpace(fromEntity))
        {
            Console.Error.WriteLine("不能同时指定 --from-table 与 --from-entity。");
            return 1;
        }

        var options = new NewModuleOptions(
            parseResult.GetValue(moduleArg)!,
            parseResult.GetValue(cnOption)!,
            parseResult.GetValue(areaOption)!,
            parseResult.GetValue(routeKeyOption),
            parseResult.GetValue(entityOption),
            parseResult.GetValue(tableOption),
            fromTable,
            fromEntity,
            parseResult.GetValue(dryRunOption),
            parseResult.GetValue(forceOption),
            backendOnly,
            frontendOnly,
            parseResult.GetValue(noMenuOption));

        var repoRoot = ScaffoldEngine.FindRepoRoot(Directory.GetCurrentDirectory());
        var engine = new ScaffoldEngine(repoRoot);
        var result = engine.Generate(options);

        Console.WriteLine($"仓库根：{repoRoot}");
        Console.WriteLine(
            $"模块：{result.Tokens.Area}/{result.Tokens.Module}（{result.Tokens.CnName}） · {result.Tokens.RoutePrefix}");
        Console.WriteLine($"字段来源：{result.FieldSource} · {result.Fields.Count} 列");
        Console.WriteLine();

        foreach (var action in result.Actions)
        {
            var relative = Path.GetRelativePath(repoRoot, action.Path);
            var suffix = string.IsNullOrEmpty(action.Detail) ? "" : $" · {action.Detail}";
            Console.WriteLine($"[{action.Action}] {relative}{suffix}");
        }

        Console.WriteLine();
        foreach (var note in result.Notes)
            Console.WriteLine(note);

        var written = result.Actions.Count(a => a.Action is "CREATE" or "OVERWRITE" or "MERGE");
        var skipped = result.Actions.Count(a => a.Action == "SKIP");
        Console.WriteLine();
        Console.WriteLine(
            options.DryRun
                ? $"dry-run 完成：将处理 {written} 个文件，跳过 {skipped} 个。"
                : $"完成：写入/合并 {written} 个文件，跳过 {skipped} 个。");
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(ex.ToString());
        return 1;
    }
});

var newCommand = new Command("new", "从模板创建代码骨架")
{
    moduleCommand
};

var root = new RootCommand("云枢 Axis 模块脚手架（模板复制 + 读表/实体字段）")
{
    newCommand
};

return root.Parse(args).Invoke();
