using dotMigrata.Simulation.Events.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Events;

/// <summary>
/// Trigger that fires once at a specific simulation tick.
/// </summary>
/// <param name="Tick">The tick number when this trigger fires.</param>
public sealed record TickTrigger(int Tick) : IEventTrigger
{
    /// <inheritdoc />
    public bool ShouldExecute(SimulationContext context)
    {
        return context.CurrentTick == Tick;
    }

    /// <inheritdoc />
    public void OnExecuted(SimulationContext context)
    {
    }
}

/// <summary>
/// Trigger that fires repeatedly at regular intervals within an optional time window.
/// </summary>
/// <param name="Interval">The number of ticks between executions.</param>
/// <param name="StartTick">Optional tick to start firing. If null, starts at tick 0.</param>
/// <param name="EndTick">Optional tick to stop firing. If null, continues indefinitely.</param>
public sealed record PeriodicTrigger(int Interval, int? StartTick = null, int? EndTick = null) : IEventTrigger
{
    /// <inheritdoc />
    public bool ShouldExecute(SimulationContext context)
    {
        if (StartTick.HasValue && context.CurrentTick < StartTick.Value)
            return false;

        if (EndTick.HasValue && context.CurrentTick > EndTick.Value)
            return false;

        return context.CurrentTick % Interval == 0;
    }

    /// <inheritdoc />
    public void OnExecuted(SimulationContext context)
    {
    }
}

/// <summary>
/// Trigger that fires continuously starting from a specific tick and optionally ending at another.
/// </summary>
/// <param name="StartTick">The tick to begin continuous execution.</param>
/// <param name="EndTick">Optional tick to stop execution. If null, continues indefinitely.</param>
public sealed record ContinuousTrigger(int StartTick, int? EndTick = null) : IEventTrigger
{
    /// <inheritdoc />
    public bool ShouldExecute(SimulationContext context)
    {
        if (context.CurrentTick < StartTick)
            return false;

        return !EndTick.HasValue || context.CurrentTick <= EndTick.Value;
    }

    /// <inheritdoc />
    public void OnExecuted(SimulationContext context)
    {
    }
}

/// <summary>
/// Trigger that fires when a custom condition evaluates to true.
/// </summary>
/// <remarks>
/// Supports optional cooldown to prevent rapid successive firings.
/// This is an extension point for complex event-condition-action patterns.
/// </remarks>
public sealed class ConditionalTrigger : IEventTrigger
{
    private int _lastExecutedTick = -1;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalTrigger" /> class.
    /// </summary>
    /// <param name="condition">The condition to evaluate each tick.</param>
    /// <param name="cooldownTicks">Optional minimum ticks between executions.</param>
    public ConditionalTrigger(Func<SimulationContext, bool> condition, int? cooldownTicks = null)
    {
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        CooldownTicks = cooldownTicks;
    }

    /// <summary>
    /// Gets the condition function that determines whether to execute.
    /// </summary>
    public Func<SimulationContext, bool> Condition { get; }

    /// <summary>
    /// Gets the optional cooldown period in ticks between executions.
    /// </summary>
    public int? CooldownTicks { get; }

    /// <inheritdoc />
    public bool ShouldExecute(SimulationContext context)
    {
        if (CooldownTicks.HasValue &&
            context.CurrentTick - _lastExecutedTick < CooldownTicks.Value)
            return false;

        return Condition(context);
    }

    /// <inheritdoc />
    public void OnExecuted(SimulationContext context)
    {
        _lastExecutedTick = context.CurrentTick;
    }
}