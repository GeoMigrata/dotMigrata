using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Simulation.Configuration;
using dotGeoMigrata.Simulation.State;
using dotGeoMigrata.Snapshot.Models;

namespace dotGeoMigrata.Snapshot.Services;

/// <summary>
/// Defines the contract for snapshot import/export services.
/// </summary>
public interface ISnapshotService
{
    /// <summary>
    /// Exports a world and optional simulation data to a snapshot.
    /// </summary>
    /// <param name="world">The world to export.</param>
    /// <param name="config">Optional simulation configuration.</param>
    /// <param name="state">Optional simulation state.</param>
    /// <returns>A WorldSnapshot containing the exported data.</returns>
    WorldSnapshot ExportToSnapshot(World world, SimulationConfiguration? config = null, SimulationState? state = null);

    /// <summary>
    /// Imports a world from a snapshot.
    /// </summary>
    /// <param name="snapshot">The snapshot to import from.</param>
    /// <returns>The imported World.</returns>
    World ImportWorld(WorldSnapshot snapshot);

    /// <summary>
    /// Imports simulation configuration from a snapshot.
    /// </summary>
    /// <param name="snapshot">The snapshot to import from.</param>
    /// <returns>The imported SimulationConfiguration, or null if not present in snapshot.</returns>
    SimulationConfiguration? ImportSimulationConfiguration(WorldSnapshot snapshot);

    /// <summary>
    /// Saves a snapshot to a file in JSON format.
    /// </summary>
    /// <param name="snapshot">The snapshot to save.</param>
    /// <param name="filePath">The file path to save to.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task SaveJsonAsync(WorldSnapshot snapshot, string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a snapshot from a JSON file.
    /// </summary>
    /// <param name="filePath">The file path to load from.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The loaded WorldSnapshot.</returns>
    Task<WorldSnapshot> LoadJsonAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves a snapshot to a file in XML format.
    /// </summary>
    /// <param name="snapshot">The snapshot to save.</param>
    /// <param name="filePath">The file path to save to.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    Task SaveXmlAsync(WorldSnapshot snapshot, string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a snapshot from an XML file.
    /// </summary>
    /// <param name="filePath">The file path to load from.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The loaded WorldSnapshot.</returns>
    Task<WorldSnapshot> LoadXmlAsync(string filePath, CancellationToken cancellationToken = default);
}