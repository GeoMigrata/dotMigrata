namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a population group definition in a snapshot.
/// </summary>
public sealed class PopulationGroupDefinitionSnapshot
{
    /// <summary>
    /// Gets or sets the display name of the population group.
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the moving willingness (0-1). Optional for backward compatibility.
    /// </summary>
    public double? MovingWillingness { get; set; }

    /// <summary>
    /// Gets or sets the retention rate (0-1). Optional for backward compatibility.
    /// </summary>
    public double? RetentionRate { get; set; }

    /// <summary>
    /// Gets or sets the factor sensitivities for this population group.
    /// Key: factor identifier or name
    /// Value: sensitivity data
    /// </summary>
    public required Dictionary<string, FactorSensitivitySnapshot> FactorSensitivities { get; set; }
}