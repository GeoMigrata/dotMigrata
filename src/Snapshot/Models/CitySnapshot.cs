namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a city in a snapshot.
/// </summary>
public sealed class CitySnapshot
{
    /// <summary>
    /// Gets or sets the display name of the city.
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the location of the city.
    /// </summary>
    public required LocationSnapshot Location { get; set; }

    /// <summary>
    /// Gets or sets the area of the city in square kilometers.
    /// </summary>
    public required double Area { get; set; }

    /// <summary>
    /// Gets or sets the factor values for this city.
    /// Key: factor identifier or name
    /// Value: factor value data
    /// </summary>
    public required Dictionary<string, FactorValueSnapshot> FactorValues { get; set; }

    /// <summary>
    /// Gets or sets the population group values for this city.
    /// Key: population group identifier or name
    /// Value: population group value data
    /// </summary>
    public required Dictionary<string, PopulationGroupValueSnapshot> PopulationGroupValues { get; set; }
}