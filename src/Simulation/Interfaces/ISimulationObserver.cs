using dotGeoMigrata.Logic.Migration;
using dotGeoMigrata.Simulation.State;

namespace dotGeoMigrata.Simulation.Interfaces;

/// <summary>
/// Observer interface for monitoring simulation progress and events.
/// Implement this interface to create custom observers that react to simulation events.
/// </summary>
public interface ISimulationObserver
{
    /// <summary>
    /// Called when the simulation starts.
    /// </summary>
    /// <param name="state">Initial simulation state.</param>
    void OnSimulationStarted(SimulationState state);

    /// <summary>
    /// Called after each simulation step completes.
    /// </summary>
    /// <param name="state">Current simulation state.</param>
    /// <param name="migrationFlows">Migration flows that occurred in this step.</param>
    void OnStepCompleted(SimulationState state, IReadOnlyList<MigrationFlow> migrationFlows);

    /// <summary>
    /// Called when the simulation completes.
    /// </summary>
    /// <param name="state">Final simulation state.</param>
    void OnSimulationCompleted(SimulationState state);
}