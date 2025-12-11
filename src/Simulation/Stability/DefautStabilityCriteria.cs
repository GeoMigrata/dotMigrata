using dotMigrata.Core.Entities;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Stability;

/// <summary>
/// Default implementation of stability criteria based on population change threshold.
/// A simulation is considered stable when the total population change across all cities
/// is below the configured threshold for the current tick.
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

        if (context.CurrentTick < config.MinTicksBeforeStabilityCheck)
            return false;

        return context.CurrentTick % config.StabilityCheckInterval == 0;
    }

    /// <inheritdoc />
    public bool IsStable(SimulationContext context, SimulationConfig config)
    {
        // Simulation is stable when total population change is at or below threshold
        return context.TotalPopulationChange <= config.StabilityThreshold;
    }

    private static int GetPreviousPopulation(City city, SimulationContext context)
    {
        // This is a simplified implementation. In a real scenario, you might want to track
        // population history in the context or use a more sophisticated approach.
        // For now, we'll assume the population change in the current tick is reflected in
        // the migration flows or other tracked data.
        return city.Population; // Placeholder - needs proper history tracking
    }
}