using dotMigrata.Core.Entities;

namespace dotMigrata.Logic.Models;

/// <summary>
/// Represents a migration decision for an individual person from an origin city to a destination city.
/// </summary>
public sealed record MigrationFlow
{
    /// <summary>
    /// Gets the origin city where the migration starts.
    /// </summary>
    public required City OriginCity { get; init; }

    /// <summary>
    /// Gets the destination city where the migration ends.
    /// </summary>
    public required City DestinationCity { get; init; }

    /// <summary>
    /// Gets the person who is migrating.
    /// </summary>
    public required Person Person { get; init; }

    /// <summary>
    /// Gets the migration probability (0-1) for this flow.
    /// </summary>
    public double MigrationProbability { get; init; }
}