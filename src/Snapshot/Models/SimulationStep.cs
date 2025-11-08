namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a single simulation step with migration events.
/// Used for incremental (delta-based) snapshot storage.
/// </summary>
public sealed record SimulationStep
{
    /// <summary>
    /// Gets or initializes the step number (0-based).
    /// </summary>
    public required int StepNumber { get; init; }

    /// <summary>
    /// Gets or initializes the timestamp when this step was executed.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or initializes the total population change in this step.
    /// </summary>
    public int TotalPopulationChange { get; init; }

    /// <summary>
    /// Gets or initializes the migration records for this step.
    /// Only migrations that actually occurred are recorded.
    /// </summary>
    public IReadOnlyList<MigrationRecord> Migrations { get; init; } = Array.Empty<MigrationRecord>();

    /// <summary>
    /// Gets or initializes optional metadata about this step.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
}