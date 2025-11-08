namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents the initial state of the world before any simulation steps.
/// This is the "seed" data from which all deltas are calculated.
/// </summary>
public sealed record InitialWorldState
{
    /// <summary>
    /// Gets or initializes the world display name.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets or initializes the factor definitions.
    /// </summary>
    public required IReadOnlyList<FactorDefinitionSnapshot> FactorDefinitions { get; init; }

    /// <summary>
    /// Gets or initializes the population group definitions.
    /// </summary>
    public required IReadOnlyList<GroupDefinitionSnapshot> GroupDefinitions { get; init; }

    /// <summary>
    /// Gets or initializes the cities with their initial states.
    /// </summary>
    public required IReadOnlyList<CitySnapshot> Cities { get; init; }
}