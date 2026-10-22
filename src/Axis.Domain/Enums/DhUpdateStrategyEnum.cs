namespace Axis.Domain.Enums;

/// <summary>
/// DataHub 更新策略。
/// </summary>
public enum DhUpdateStrategyEnum
{
    Full = 1,
    Incremental = 2,
    DiffDetect = 3
}
