namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents the initialization data of the world, including all structural definitions.
/// </summary>
public class InitializationSnapshot
{
    /// <summary>
    /// Gets or sets the factor definitions in the world.
    /// Key: factor identifier (for XML) or factor name (for JSON)
    /// Value: factor definition data
    /// </summary>
    public required Dictionary<string, FactorDefinitionSnapshot> FactorDefinitions { get; set; }

    /// <summary>
    /// Gets or sets the population group definitions in the world.
    /// Key: population group identifier (for XML) or group name (for JSON)
    /// Value: population group definition data
    /// </summary>
    public required Dictionary<string, PopulationGroupDefinitionSnapshot> PopulationGroupDefinitions { get; set; }

    /// <summary>
    /// Gets or sets the cities in the world.
    /// Key: city identifier (for XML) or city name (for JSON)
    /// Value: city data
    /// </summary>
    public required Dictionary<string, CitySnapshot> Cities { get; set; }
}