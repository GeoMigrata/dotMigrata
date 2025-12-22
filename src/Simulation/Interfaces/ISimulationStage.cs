using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Interfaces;

/// <summary>
/// Represents a single stage in the simulation pipeline.
/// </summary>
/// <remarks>
/// Stages are executed in sequence during each simulation step.
/// </remarks>
public interface ISimulationStage
{
    /// <summary>
    /// Gets the name of this simulation stage for identification and logging.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes this stage of the simulation.
    /// </summary>
    /// <param name="context">The current simulation context containing world state and step information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ExecuteAsync(SimulationContext context);

    /// <summary>
    /// Determines whether this stage should be executed for the current step.
    /// </summary>
    /// <param name="context">The current simulation context.</param>
    /// <returns>
    /// <see langword="true" /> if the stage should execute;
    /// <see langword="false" /> to skip the stage.
    /// </returns>
    /// <remarks>
    /// Can be used to skip stages conditionally (for example, feedback only every N steps).
    /// The default implementation returns <see langword="true" />.
    /// </remarks>
    bool ShouldExecute(SimulationContext context) => true;
}