namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a complete snapshot of the simulation world, including initialization data,
/// simulation configuration, and simulation state.
/// </summary>
public sealed class WorldSnapshot
{
    /// <summary>
    /// Gets or sets the display name of the world.
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the version of the snapshot format.
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Gets or sets the initialization data containing world structure (cities, factors, population groups).
    /// </summary>
    public required InitializationSnapshot Initialization { get; set; }

    /// <summary>
    /// Gets or sets the simulation configuration parameters.
    /// </summary>
    public SimulationConfigSnapshot? SimulationConfig { get; set; }

    /// <summary>
    /// Gets or sets the current simulation state.
    /// </summary>
    public SimulationStateSnapshot? Simulation { get; set; }
}