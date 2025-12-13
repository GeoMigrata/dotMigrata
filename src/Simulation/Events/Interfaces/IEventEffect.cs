using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Events.Interfaces;

/// <summary>
/// Defines the effect applied when an event executes.
/// </summary>
/// <remarks>
/// Effects modify world state, typically by changing city factor values.
/// Implementations can support immediate changes, transitions over time, or complex calculations.
/// </remarks>
public interface IEventEffect
{
    /// <summary>
    /// Applies the effect to the simulation world.
    /// </summary>
    /// <param name="context">The current simulation context.</param>
    void Apply(SimulationContext context);
}