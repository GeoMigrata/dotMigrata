using dotMigrata.Simulation.Events.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Events.Triggers;

/// <summary>
/// Trigger that fires when a custom condition evaluates to true.
/// </summary>
/// <remarks>
/// Supports optional cooldown to prevent rapid successive firings.
/// This is an extension point for complex event-condition-action patterns.
/// Thread-safe implementation using Interlocked operations for parallel execution.
/// </remarks>
public sealed class ConditionalTrigger : IEventTrigger
{
    private int _lastExecutedStep = -1;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConditionalTrigger" /> class.
    /// </summary>
    /// <param name="condition">The condition to evaluate each step.</param>
    /// <param name="cooldownSteps">Optional minimum steps between executions.</param>
    public ConditionalTrigger(Func<SimulationContext, bool> condition, int? cooldownSteps = null)
    {
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        CooldownSteps = cooldownSteps;
    }

    /// <summary>
    /// Gets the condition function that determines whether to execute.
    /// </summary>
    public Func<SimulationContext, bool> Condition { get; }

    /// <summary>
    /// Gets the optional cooldown period in steps between executions.
    /// </summary>
    public int? CooldownSteps { get; }

    /// <inheritdoc />
    public bool ShouldExecute(SimulationContext context)
    {
        if (CooldownSteps.HasValue &&
            context.CurrentStep - Volatile.Read(ref _lastExecutedStep) < CooldownSteps.Value)
            return false;

        return Condition(context);
    }

    /// <inheritdoc />
    public void OnExecuted(SimulationContext context) =>
        Interlocked.Exchange(ref _lastExecutedStep, context.CurrentStep);
}