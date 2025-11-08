using dotGeoMigrata.Simulation.Models;

namespace dotGeoMigrata.Simulation.Interfaces;

/// <summary>
/// Observer interface for monitoring simulation progress and events.
/// Implementations can react to simulation events for logging, visualization, or analysis.
/// </summary>
public interface ISimulationObserver
{
    /// <summary>
    /// Called when the simulation starts.
    /// </summary>
    /// <param name="context">The initial simulation context.</param>
    void OnSimulationStart(SimulationContext context);

    /// <summary>
    /// Called at the beginning of each simulation tick.
    /// </summary>
    /// <param name="context">The current simulation context.</param>
    void OnTickStart(SimulationContext context);

    /// <summary>
    /// Called when a simulation stage completes.
    /// </summary>
    /// <param name="stageName">The name of the completed stage.</param>
    /// <param name="context">The current simulation context.</param>
    void OnStageComplete(string stageName, SimulationContext context);

    /// <summary>
    /// Called at the end of each simulation tick.
    /// </summary>
    /// <param name="context">The current simulation context.</param>
    void OnTickComplete(SimulationContext context);

    /// <summary>
    /// Called when the simulation completes or is terminated.
    /// </summary>
    /// <param name="context">The final simulation context.</param>
    /// <param name="reason">The reason for termination (e.g., "MaxTicksReached", "Stabilized", "Stopped").</param>
    void OnSimulationEnd(SimulationContext context, string reason);

    /// <summary>
    /// Called when an error occurs during simulation.
    /// </summary>
    /// <param name="context">The current simulation context.</param>
    /// <param name="exception">The exception that occurred.</param>
    void OnError(SimulationContext context, Exception exception);
}