using dotMigrata.Simulation.Events.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Events.Triggers;

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