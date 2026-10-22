using Axis.Application.Author.Dto;
using FastEndpoints;
using FluentValidation;

namespace Axis.Application.Author.Validators;

/// <summary>
/// <see cref="LoginRequest"/> 的入参格式校验。
/// </summary>
/// <remarks>
/// 仅做格式与必填；验证码正确性、密码校验与锁定由 <see cref="IAuthorAppService"/> 负责。
/// </remarks>
public sealed class LoginRequestValidator : Validator<LoginRequest>
{
    /// <summary>
    /// 注册字段规则。
    /// </summary>
    public LoginRequestValidator()
    {
        RuleFor(x => x.Account)
            .NotEmpty().WithMessage("账号不能为空")
            .MinimumLength(2).WithMessage("账号不能少于两个字符")
            .MaximumLength(32).WithMessage("账号不能超过 32 个字符");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空")
            .MaximumLength(1024).WithMessage("密码密文不能超过 1024 个字符");

        RuleFor(x => x.CodeId)
            .NotEmpty().WithMessage("验证码 Id 不能为空");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("验证码不能为空");
    }
}
