using dotMigrata.Simulation.Events.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Events.Triggers;

/// <summary>
/// Trigger that fires repeatedly at regular intervals within an optional time window.
/// </summary>
/// <param name="Interval">The number of steps between executions.</param>
/// <param name="StartStep">Optional step to start firing. If null, starts at step 0.</param>
/// <param name="EndStep">Optional step to stop firing. If null, continues indefinitely.</param>
public sealed record PeriodicTrigger(int Interval, int? StartStep = null, int? EndStep = null) : IEventTrigger
{
    /// <inheritdoc />
    public bool ShouldExecute(SimulationContext context)
    {
        if (StartStep.HasValue && context.CurrentStep < StartStep.Value)
            return false;

        if (EndStep.HasValue && context.CurrentStep > EndStep.Value)
            return false;

        return context.CurrentStep % Interval == 0;
    }

    /// <inheritdoc />
    public void OnExecuted(SimulationContext context)
    {
    }
}