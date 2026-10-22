namespace Axis.Domain.System;

/// <summary>
/// 平台内置编码规则的调用键。
/// </summary>
/// <remarks>
/// 业务模块取号必须引用本常量（或各域 <c>*CodeRuleCodes</c>），禁止散落魔法字符串。
/// 种子规则的 <c>Code</c> 与此保持一致，管理端不可改编码。
/// </remarks>
public static class CodeRuleCodes
{
    /// <summary>
    /// 物料编码规则。
    /// </summary>
    public const string Material = "MAT";

    /// <summary>
    /// 工序编码规则。
    /// </summary>
    public const string Process = "PRC";

    /// <summary>
    /// 工艺路线编码规则。
    /// </summary>
    public const string Routing = "RTG";

    /// <summary>
    /// 生产工单号规则。
    /// </summary>
    public const string WorkOrder = "WO";

    /// <summary>
    /// 报工单号规则。
    /// </summary>
    public const string Report = "RPT";

    /// <summary>
    /// 检验单号规则。
    /// </summary>
    public const string Inspection = "IQC";
}
