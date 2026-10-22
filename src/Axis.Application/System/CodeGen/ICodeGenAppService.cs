using Axis.Application.System.CodeGen.Dto;

namespace Axis.Application.System.CodeGen;

/// <summary>
/// 管理端代码生成（薄壳，底层复用 Axis.Scaffold 引擎）。
/// </summary>
public interface ICodeGenAppService
{
    Task<CodeGenStatusDto> GetStatusAsync(CancellationToken ct = default);

    Task<IReadOnlyList<string>> ListTablesAsync(CancellationToken ct = default);

    Task<IReadOnlyList<string>> ListEntitiesAsync(CancellationToken ct = default);

    Task<CodeGenResultDto> PreviewAsync(GenerateModuleInput input, CancellationToken ct = default);

    Task<CodeGenResultDto> GenerateAsync(GenerateModuleInput input, CancellationToken ct = default);
}
