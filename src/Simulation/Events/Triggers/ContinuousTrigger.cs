using dotMigrata.Simulation.Events.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Events.Triggers;

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