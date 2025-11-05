using System.Text.Json.Serialization;

namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a complete snapshot of the simulation world, including initialization data,
/// simulation configuration, and simulation state.
/// </summary>
/// <remarks>
/// In JSON format, metadata fields like version and initialization are prefixed with underscore
/// to distinguish them from actual data fields. In code, use standard property names.
/// </remarks>
public sealed class WorldSnapshot
{
    /// <summary>
    /// Gets or sets the display name of the world.
    /// In JSON, serialized as "_version" to indicate it's metadata.
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the version of the snapshot format.
    /// </summary>
    [JsonPropertyName("_version")]
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Gets or sets the initialization data containing world structure (cities, factors, population groups).
    /// In JSON, serialized as "_initialization" to indicate it's metadata/structure.
    /// </summary>
    [JsonPropertyName("_initialization")]
    public required InitializationSnapshot Initialization { get; set; }

    /// <summary>
    /// Gets or sets the simulation configuration parameters.
    /// In JSON, serialized as "_simulationConfig" to indicate it's metadata/configuration.
    /// </summary>
    [JsonPropertyName("_simulationConfig")]
    public SimulationConfigSnapshot? SimulationConfig { get; set; }

    /// <summary>
    /// Gets or sets the current simulation state.
    /// In JSON, serialized as "_simulation" to indicate its metadata/state.
    /// </summary>
    [JsonPropertyName("_simulation")]
    public SimulationStateSnapshot? Simulation { get; set; }
}