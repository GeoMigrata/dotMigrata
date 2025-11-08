using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Values;

namespace dotGeoMigrata.Logic.Models;

/// <summary>
/// Represents a migration flow from an origin city to a destination city for a specific population group.
/// </summary>
public sealed record MigrationFlow
{
    /// <summary>
    /// gets the origin city where the migration starts.
    /// </summary>
    public required City OriginCity { get; init; }

    /// <summary>
    /// Gets the destination city where the migration ends.
    /// </summary>
    public required City DestinationCity { get; init; }

    /// <summary>
    /// Gets the population group that is migrating.
    /// </summary>
    public required GroupDefinition Group { get; init; }

    /// <summary>
    /// Gets the number of people migrating (can be fractional for probabilistic calculations).
    /// </summary>
    public required double MigrationCount { get; init; }

    /// <summary>
    /// Gets the migration probability (0-1) for this flow.
    /// </summary>
    public double MigrationProbability { get; init; }
}