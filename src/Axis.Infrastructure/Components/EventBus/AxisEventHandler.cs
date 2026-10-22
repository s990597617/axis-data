using FastEndpoints;

namespace Axis.Infrastructure.Components.EventBus;

/// <summary>
/// 领域事件处理器基类。
/// <para>
/// 封装 FastEndpoints <see cref="IEventHandler{TEvent}"/>，
/// 提供日志、异常兜底等统一行为，业务方只需关注 <see cref="OnHandleAsync"/> 逻辑。
/// </para>
/// <para>
/// FastEndpoints 自动扫描并注册所有 <see cref="IEventHandler{TEvent}"/> 实现，
/// 无需手动订阅。一个事件可以有多个 Handler。
/// </para>
/// </summary>
/// <typeparam name="TEvent">事件类型</typeparam>
public abstract class AxisEventHandler<TEvent> : IEventHandler<TEvent> where TEvent : IAxisEvent
{
    /// <summary>
    /// FastEndpoints 调用入口，内部包裹异常处理后委托给 <see cref="OnHandleAsync"/>。
    /// </summary>
    public async Task HandleAsync(TEvent eventModel, CancellationToken ct)
    {
        var handlerName = GetType().Name;
        var logger = GlobalContext.RootServices?.GetService<ILoggerFactory>()
            ?.CreateLogger(GetType());

        try
        {
            logger?.LogDebug("事件处理开始：{Handler} ← {Event}（Source={Source}）",
                handlerName, typeof(TEvent).Name, eventModel.Source);

            await OnHandleAsync(eventModel, ct);

            logger?.LogDebug("事件处理完成：{Handler} ← {Event}", handlerName, typeof(TEvent).Name);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "事件处理失败：{Handler} ← {Event}（Source={Source}）",
                handlerName, typeof(TEvent).Name, eventModel.Source);

            // 默认不抛出，防止影响其他 Handler；子类可 override 改变行为
            if (ShouldRethrow) throw;
        }
    }

    /// <summary>
    /// 业务方实现的事件处理逻辑
    /// </summary>
    /// <param name="event">事件模型</param>
    /// <param name="ct">取消令牌</param>
    protected abstract Task OnHandleAsync(TEvent @event, CancellationToken ct);

    /// <summary>
    /// 处理失败时是否重新抛出异常（默认 false，即吞掉异常仅记录日志）。
    /// <para>设为 true 时，若使用 Mode.WaitForAll 发布，调用方将感知到异常。</para>
    /// </summary>
    protected virtual bool ShouldRethrow => false;
}
