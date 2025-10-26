namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a factor sensitivity in a snapshot.
/// </summary>
public sealed class FactorSensitivitySnapshot
{
    /// <summary>
    /// Gets or sets the sensitivity value.
    /// </summary>
    public required int Sensitivity { get; set; }

    /// <summary>
    /// Gets or sets the optional overridden factor type.
    /// </summary>
    public string? OverriddenFactorType { get; set; }
}