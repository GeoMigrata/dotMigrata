using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Interfaces;

/// <summary>
/// Provides lifecycle hooks for simulation stages.
/// Implement this interface to receive notifications when the simulation starts and ends.
/// </summary>
public interface ISimulationStageLifecycle
{
    /// <summary>
    /// Called once when the simulation starts, before the first tick.
    /// </summary>
    /// <param name="context">The initial simulation context.</param>
    void OnSimulationStart(SimulationContext context)
    {
    }

    /// <summary>
    /// Called once when the simulation ends, after the last tick.
    /// This is called regardless of whether the simulation ended normally, due to stability, or due to an exception.
    /// </summary>
    /// <param name="context">The final simulation context.</param>
    void OnSimulationEnd(SimulationContext context)
    {
    }
}