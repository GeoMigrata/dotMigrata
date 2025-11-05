namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a factor sensitivity in a snapshot.
/// </summary>
/// <remarks>
/// Factor sensitivity defines how much a population group is affected by a specific factor.
/// Higher absolute values indicate stronger influence on migration decisions.
/// </remarks>
public sealed class FactorSensitivitySnapshot
{
    /// <summary>
    /// Gets or sets the sensitivity value.
    /// </summary>
    /// <value>
    /// An integer value indicating the weight of this factor for the population group.
    /// Higher values indicate stronger influence on migration decisions.
    /// </value>
    public required int Sensitivity { get; set; }

    /// <summary>
    /// Gets or sets the optional overridden factor type.
    /// </summary>
    /// <value>
    /// When specified, overrides the factor definition's type for this specific population group.
    /// Useful when a factor that's generally positive needs to be treated as negative for certain groups.
    /// Valid values: "Positive", "Negative" or null to use the factor's default type.
    /// </value>
    public string? OverriddenFactorType { get; set; }
}