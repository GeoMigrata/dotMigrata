namespace dotGeoMigrata.Simulation.Interfaces;

/// <summary>
/// Manages and executes a collection of simulation stages in a pipeline architecture.
/// </summary>
public interface ISimulationPipeline
{
    /// <summary>
    /// Gets the collection of stages in this pipeline, ordered by priority.
    /// </summary>
    IReadOnlyList<ISimulationStage> Stages { get; }

    /// <summary>
    /// Adds a stage to the pipeline.
    /// </summary>
    /// <param name="stage">The stage to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when stage is null.</exception>
    void AddStage(ISimulationStage stage);

    /// <summary>
    /// Removes a stage from the pipeline.
    /// </summary>
    /// <param name="stage">The stage to remove.</param>
    /// <exception cref="ArgumentNullException">Thrown when stage is null.</exception>
    /// <returns>True if the stage was removed, false if it was not found.</returns>
    bool RemoveStage(ISimulationStage stage);

    /// <summary>
    /// Executes all stages in the pipeline in order.
    /// </summary>
    /// <param name="context">The simulation context.</param>
    /// <returns>True if all stages executed successfully, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    bool Execute(SimulationContext context);
}