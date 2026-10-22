using Axis.Domain.System;

namespace Axis.Domain.Mdm;

/// <summary>
/// 主数据域编码规则调用键。
/// </summary>
/// <remarks>
/// 取值必须与平台 <see cref="CodeRuleCodes"/> / 种子规则编码一致，禁止魔法字符串。
/// </remarks>
public static class MdmCodeRuleCodes
{
    /// <summary>
    /// 物料编码规则。
    /// </summary>
    public const string Material = CodeRuleCodes.Material;

    /// <summary>
    /// 工序编码规则。
    /// </summary>
    public const string Process = CodeRuleCodes.Process;

    /// <summary>
    /// 工艺路线编码规则。
    /// </summary>
    public const string Routing = CodeRuleCodes.Routing;
}
