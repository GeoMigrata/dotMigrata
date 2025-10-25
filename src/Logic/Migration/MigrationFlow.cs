using dotGeoMigrata.Core.Entities;

namespace dotGeoMigrata.Logic.Migration;

/// <summary>
/// Represents a migration flow from one city to another for a specific population group.
/// </summary>
public sealed record MigrationFlow
{
    /// <summary>
    /// The source city from which people are migrating.
    /// </summary>
    public required City SourceCity { get; init; }

    /// <summary>
    /// The destination city to which people are migrating.
    /// </summary>
    public required City DestinationCity { get; init; }

    /// <summary>
    /// The population group that is migrating.
    /// </summary>
    public required PopulationGroup PopulationGroup { get; init; }

    /// <summary>
    /// The number of people migrating.
    /// </summary>
    public required int MigrantCount { get; init; }

    /// <summary>
    /// The calculated migration probability (0-1) before sampling.
    /// </summary>
    public double MigrationProbability { get; init; }

    /// <summary>
    /// The attraction difference that motivated this migration.
    /// </summary>
    public double AttractionDifference { get; init; }
}