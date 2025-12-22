using dotMigrata.Simulation.Events.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Events.Triggers;

/// <summary>
/// Trigger that fires continuously starting from a specific step and optionally ending at another.
/// </summary>
/// <param name="StartStep">The step to begin continuous execution.</param>
/// <param name="EndStep">Optional step to stop execution. If null, continues indefinitely.</param>
public sealed record ContinuousTrigger(int StartStep, int? EndStep = null) : IEventTrigger
{
    /// <inheritdoc />
    public bool ShouldExecute(SimulationContext context)
    {
        if (context.CurrentStep < StartStep)
            return false;

        return !EndStep.HasValue || context.CurrentStep <= EndStep.Value;
    }

    /// <inheritdoc />
    public void OnExecuted(SimulationContext context)
    {
    }
}