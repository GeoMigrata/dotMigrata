using dotMigrata.Simulation.Events.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Events.Triggers;

/// <summary>
/// Trigger that fires once at a specific simulation step.
/// </summary>
/// <param name="Step">The step number when this trigger fires.</param>
public sealed record StepTrigger(int Step) : IEventTrigger
{
    /// <inheritdoc />
    public bool ShouldExecute(SimulationContext context) => context.CurrentStep == Step;

    /// <inheritdoc />
    public void OnExecuted(SimulationContext context)
    {
    }
}