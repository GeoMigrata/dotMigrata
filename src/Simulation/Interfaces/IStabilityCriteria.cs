using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Interfaces;

/// <summary>
/// Defines a strategy for determining when a simulation has stabilized.
/// </summary>
public interface IStabilityCriteria
{
    /// <summary>
    /// Determines whether stability should be checked at the current step.
    /// </summary>
    /// <param name="context">The current simulation context.</param>
    /// <param name="config">The simulation configuration.</param>
    /// <returns>
    /// <see langword="true" /> if stability should be checked; otherwise, <see langword="false" />.
    /// </returns>
    bool ShouldCheckStability(SimulationContext context, SimulationConfig config);

    /// <summary>
    /// Determines whether the simulation is considered stable.
    /// </summary>
    /// <param name="context">The current simulation context.</param>
    /// <param name="config">The simulation configuration.</param>
    /// <returns>
    /// <see langword="true" /> if the simulation is stable; otherwise, <see langword="false" />.
    /// </returns>
    bool IsStable(SimulationContext context, SimulationConfig config);
}