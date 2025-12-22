using dotMigrata.Core.Entities;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Stability;

/// <summary>
/// Default implementation of stability criteria based on population change threshold.
/// A simulation is considered stable when the total population change across all cities
/// is below the configured threshold for the current step.
/// </summary>
/// <remarks>
/// This implementation checks whether the <see cref="SimulationContext.TotalPopulationChange" />
/// is less than or equal to the configured <see cref="SimulationConfig.StabilityThreshold" />.
/// </remarks>
public sealed class DefaultStabilityCriteria : IStabilityCriteria
{
    /// <inheritdoc />
    public bool ShouldCheckStability(SimulationContext context, SimulationConfig config)
    {
        if (!config.CheckStability)
            return false;

        if (context.CurrentStep < config.MinStepsBeforeStabilityCheck)
            return false;

        return context.CurrentStep % config.StabilityCheckInterval == 0;
    }

    /// <inheritdoc />
    public bool IsStable(SimulationContext context, SimulationConfig config) =>
        context.TotalPopulationChange <=
        config.StabilityThreshold; // Simulation is stable when total population change is at or below threshold

    // This is a simplified implementation. In a real scenario, you might want to track
    // population history in the context or use a more sophisticated approach.
    // For now, we'll assume the population change in the current step is reflected in
    // the migration flows or other tracked data.
    // Placeholder - needs proper history tracking
    private static int GetPreviousPopulation(City city, SimulationContext context) => city.Population;
}