using Axis.Application.System.CodeGen.Dto;
using Axis.Infrastructure.Dependency.Annotations;
using Axis.Infrastructure.Exceptions;
using Axis.Infrastructure.Options;
using Axis.Scaffold;
using Axis.Scaffold.Schema;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace Axis.Application.System.CodeGen;

/// <summary>
/// 管理端代码生成：预览 dry-run / 落盘生成 / 列库表与实体。
/// </summary>
public sealed class CodeGenAppService(
    ISqlSugarClient db,
    IOptions<AxisCodeGenOptions> options,
    IWebHostEnvironment environment)
    : ICodeGenAppService, IAutoInject
{
    /// <inheritdoc />
    public Task<CodeGenStatusDto> GetStatusAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        var opt = options.Value;
        var isProd = environment.IsProduction();
        string repoRoot;
        try
        {
            repoRoot = ResolveRepoRoot();
        }
        catch (Exception ex)
        {
            return Task.FromResult(new CodeGenStatusDto
            {
                Enabled = opt.Enabled,
                AllowInProduction = opt.AllowInProduction,
                IsProduction = isProd,
                CanGenerate = false,
                RepoRoot = "",
                Message = $"仓库根未解析：{ex.Message}"
            });
        }

        var can = opt.Enabled && (!isProd || opt.AllowInProduction);
        return Task.FromResult(new CodeGenStatusDto
        {
            Enabled = opt.Enabled,
            AllowInProduction = opt.AllowInProduction,
            IsProduction = isProd,
            CanGenerate = can,
            RepoRoot = repoRoot,
            Message = can
                ? "代码生成已启用（开发环境建议先预览再生成）。"
                : isProd && !opt.AllowInProduction
                    ? "生产环境默认禁止代码生成。"
                    : "代码生成已关闭（CodeGen:Enabled=false）。"
        });
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> ListTablesAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        EnsureCanGenerate();
        try
        {
            return Task.FromResult(TableSchemaReader.ListTables(db));
        }
        catch (Exception ex)
        {
            throw Wrap(ex);
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> ListEntitiesAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        EnsureCanGenerate();
        try
        {
            var repoRoot = ResolveRepoRoot();
            return Task.FromResult(EntitySchemaReader.ListEntityNames(repoRoot));
        }
        catch (Exception ex)
        {
            throw Wrap(ex);
        }
    }

    /// <inheritdoc />
    public Task<CodeGenResultDto> PreviewAsync(GenerateModuleInput input, CancellationToken ct = default)
        => RunAsync(input, dryRun: true, ct);

    /// <inheritdoc />
    public Task<CodeGenResultDto> GenerateAsync(GenerateModuleInput input, CancellationToken ct = default)
        => RunAsync(input, dryRun: false, ct);

    private Task<CodeGenResultDto> RunAsync(GenerateModuleInput input, bool dryRun, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        EnsureCanGenerate();

        try
        {
            var repoRoot = ResolveRepoRoot();
            var engine = new ScaffoldEngine(repoRoot);
            var optionsModel = new NewModuleOptions(
                input.Module.Trim(),
                input.CnName.Trim(),
                string.IsNullOrWhiteSpace(input.Area) ? "Demo" : input.Area.Trim(),
                string.IsNullOrWhiteSpace(input.RouteKey) ? null : input.RouteKey.Trim(),
                string.IsNullOrWhiteSpace(input.Entity) ? null : input.Entity.Trim(),
                string.IsNullOrWhiteSpace(input.Table) ? null : input.Table.Trim(),
                string.IsNullOrWhiteSpace(input.FromTable) ? null : input.FromTable.Trim(),
                string.IsNullOrWhiteSpace(input.FromEntity) ? null : input.FromEntity.Trim(),
                dryRun,
                input.Force,
                input.BackendOnly,
                input.FrontendOnly,
                input.NoMenu);

            var result = engine.Generate(optionsModel, db);
            return Task.FromResult(Map(result, dryRun, repoRoot));
        }
        catch (Exception ex) when (ex is not BusinessException)
        {
            throw Wrap(ex);
        }
    }

    private void EnsureCanGenerate()
    {
        var opt = options.Value;
        if (!opt.Enabled)
            throw new BusinessException("代码生成已关闭（请在 Config/08-CodeGen.json 设置 CodeGen:Enabled=true）");

        if (environment.IsProduction() && !opt.AllowInProduction)
            throw new BusinessException("生产环境默认禁止代码生成");
    }

    private string ResolveRepoRoot()
    {
        var configured = options.Value.RepoRootPath?.Trim();
        if (!string.IsNullOrWhiteSpace(configured))
        {
            var full = Path.GetFullPath(configured);
            if (!global::System.IO.File.Exists(Path.Combine(full, "Axis.Net.sln")))
                throw new BusinessException($"CodeGen:RepoRootPath 无效（未找到 Axis.Net.sln）：{full}");
            return full;
        }

        return ScaffoldEngine.FindRepoRoot(environment.ContentRootPath);
    }

    private static CodeGenResultDto Map(ScaffoldResult result, bool dryRun, string repoRoot)
        => new()
        {
            Area = result.Tokens.Area,
            Module = result.Tokens.Module,
            CnName = result.Tokens.CnName,
            RoutePrefix = result.Tokens.RoutePrefix,
            FieldSource = result.FieldSource,
            DryRun = dryRun,
            Fields = result.Fields.Select(f => new CodeGenFieldDto
            {
                Name = f.Name,
                CsType = f.CsType,
                TsType = f.TsType,
                Label = f.Label
            }).ToList(),
            Actions = result.Actions.Select(a =>
            {
                var relative = Path.GetRelativePath(repoRoot, a.Path).Replace('\\', '/');
                return new CodeGenFileActionDto
                {
                    Path = relative,
                    Action = a.Action,
                    Detail = a.Detail,
                    Content = a.Content,
                    Language = DetectLanguage(relative)
                };
            }).ToList(),
            Notes = result.Notes.ToList()
        };

    private static string DetectLanguage(string relativePath)
    {
        var ext = Path.GetExtension(relativePath).ToLowerInvariant();
        return ext switch
        {
            ".cs" => "csharp",
            ".ts" => "typescript",
            ".tsx" => "typescript",
            ".vue" => "vue",
            ".scss" => "scss",
            ".css" => "css",
            ".json" => "json",
            ".md" => "markdown",
            _ => "text"
        };
    }

    private static BusinessException Wrap(Exception ex)
        => new(ex.Message);
}
