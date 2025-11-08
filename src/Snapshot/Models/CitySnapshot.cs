namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Snapshot representation of a city.
/// </summary>
public record CitySnapshot
{
    /// <summary>
    /// Gets or initializes the display name.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets or initializes the city area in square kilometers.
    /// </summary>
    public required double Area { get; init; }

    /// <summary>
    /// Gets or initializes the location coordinates.
    /// </summary>
    public required LocationSnapshot Location { get; init; }

    /// <summary>
    /// Gets or initializes the capacity (optional).
    /// </summary>
    public int? Capacity { get; init; }

    /// <summary>
    /// Gets or initializes the factor values.
    /// Maps factor display name to intensity value.
    /// </summary>
    public required IReadOnlyList<FactorValueSnapshot> FactorValues { get; init; }

    /// <summary>
    /// Gets or initializes the population group values.
    /// Maps group display name to population count.
    /// </summary>
    public required IReadOnlyList<GroupValueSnapshot> GroupValues { get; init; }
}