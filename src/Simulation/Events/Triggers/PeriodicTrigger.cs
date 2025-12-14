using dotMigrata.Simulation.Events.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Events.Triggers;

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