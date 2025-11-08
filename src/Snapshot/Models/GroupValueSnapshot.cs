namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Snapshot representation of a population group value.
/// </summary>
public sealed record GroupValueSnapshot
{
    /// <summary>
    /// Gets or initializes the group reference (display name).
    /// </summary>
    public required string GroupRef { get; init; }

    /// <summary>
    /// Gets or initializes the population count.
    /// </summary>
    public required int Population { get; init; }
}