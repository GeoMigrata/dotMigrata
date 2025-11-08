namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Snapshot representation of a factor sensitivity.
/// </summary>
public sealed record FactorSensitivitySnapshot
{
    /// <summary>
    /// Gets or initializes the factor reference (display name).
    /// </summary>
    public required string FactorRef { get; init; }

    /// <summary>
    /// Gets or initializes the sensitivity value.
    /// </summary>
    public required int Sensitivity { get; init; }

    /// <summary>
    /// Gets or initializes an optional factor type override ("Positive" or "Negative").
    /// </summary>
    public string? OverriddenFactorType { get; init; }
}