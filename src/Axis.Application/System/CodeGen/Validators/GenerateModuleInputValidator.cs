using Axis.Application.System.CodeGen.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.System.CodeGen.Validators;

/// <summary>
/// 模块代码生成入参校验。
/// </summary>
public sealed class GenerateModuleInputValidator : Validator<GenerateModuleInput>
{
    public GenerateModuleInputValidator()
    {
        RuleFor(x => x.Module)
            .NotEmpty().WithMessage("模块名不能为空")
            .Matches(@"^[A-Z][A-Za-z0-9]*$").WithMessage("模块名须为 PascalCase，如 Product");

        RuleFor(x => x.CnName)
            .NotEmpty().WithMessage("中文名不能为空")
            .MaximumLength(32).WithMessage("中文名不能超过 32 字");

        RuleFor(x => x.Area)
            .NotEmpty().WithMessage("业务域不能为空")
            .Matches(@"^[A-Z][A-Za-z0-9]*$").WithMessage("业务域须为 PascalCase，如 Demo");

        RuleFor(x => x.RouteKey)
            .Matches(@"^[a-z][a-z0-9-]*$").WithMessage("route-key 须为 kebab，如 product")
            .When(x => !string.IsNullOrWhiteSpace(x.RouteKey));

        RuleFor(x => x.Entity)
            .Matches(@"^[A-Z][A-Za-z0-9]*$").WithMessage("实体名须为 PascalCase")
            .When(x => !string.IsNullOrWhiteSpace(x.Entity));

        RuleFor(x => x)
            .Must(x => string.IsNullOrWhiteSpace(x.FromTable) || string.IsNullOrWhiteSpace(x.FromEntity))
            .WithMessage("不能同时指定 FromTable 与 FromEntity");

        RuleFor(x => x)
            .Must(x => !(x.BackendOnly && x.FrontendOnly))
            .WithMessage("不能同时指定 BackendOnly 与 FrontendOnly");
    }
}
