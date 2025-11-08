using dotGeoMigrata.Simulation.Models;

namespace dotGeoMigrata.Simulation.Interfaces;

/// <summary>
/// Represents a single stage in the simulation pipeline.
/// Stages are executed in sequence during each simulation tick.
/// </summary>
public interface ISimulationStage
{
    /// <summary>
    /// Gets the name of this simulation stage for identification and logging.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes this stage of the simulation.
    /// </summary>
    /// <param name="context">The current simulation context containing world state and tick information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ExecuteAsync(SimulationContext context);

    /// <summary>
    /// Determines whether this stage should be executed for the current tick.
    /// Can be used to skip stages conditionally (e.g., feedback only every N ticks).
    /// </summary>
    /// <param name="context">The current simulation context.</param>
    /// <returns>True if the stage should execute; false to skip.</returns>
    bool ShouldExecute(SimulationContext context)
    {
        return true;
    }
}