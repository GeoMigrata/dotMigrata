using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Simulation.State;

namespace dotGeoMigrata.Simulation.Interfaces;

/// <summary>
/// Represents a single stage in the simulation pipeline.
/// Each stage performs a specific operation in the simulation workflow.
/// </summary>
public interface ISimulationStage
{
    /// <summary>
    /// Gets the name of this simulation stage.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the order priority of this stage in the pipeline.
    /// Stages are executed in ascending order of priority.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Executes this stage of the simulation.
    /// </summary>
    /// <param name="context">The simulation context containing world state and runtime information.</param>
    /// <returns>The result of executing this stage.</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    SimulationStageResult Execute(SimulationContext context);
}

/// <summary>
/// Context object that flows through the simulation pipeline,
/// containing the world state and accumulated stage results.
/// </summary>
public sealed class SimulationContext
{
    /// <summary>
    /// Gets the world being simulated.
    /// </summary>
    public required World World { get; init; }

    /// <summary>
    /// Gets the current simulation state.
    /// </summary>
    public required SimulationState State { get; init; }

    /// <summary>
    /// Gets the random number generator for this simulation.
    /// </summary>
    public required Random Random { get; init; }

    /// <summary>
    /// Gets or initializes the shared data dictionary for passing data between stages.
    /// </summary>
    public Dictionary<string, object> SharedData { get; init; } = new();
}

/// <summary>
/// Result returned by a simulation stage execution.
/// </summary>
public sealed class SimulationStageResult
{
    /// <summary>
    /// Gets whether the stage execution was successful.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Gets optional message about the stage execution.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Gets optional data produced by this stage.
    /// </summary>
    public object? Data { get; init; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <param name="message">Optional success message.</param>
    /// <param name="data">Optional result data.</param>
    /// <returns>A successful stage result.</returns>
    public static SimulationStageResult Successful(string? message = null, object? data = null)
    {
        return new SimulationStageResult { Success = true, Message = message, Data = data };
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <returns>A failed stage result.</returns>
    public static SimulationStageResult Failed(string message)
    {
        return new SimulationStageResult { Success = false, Message = message };
    }
}