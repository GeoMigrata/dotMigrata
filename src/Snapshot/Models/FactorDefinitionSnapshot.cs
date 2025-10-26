namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Represents a factor definition in a snapshot.
/// </summary>
public sealed class FactorDefinitionSnapshot
{
    /// <summary>
    /// Gets or sets the display name of the factor.
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the type of the factor (Positive/Negative or Benefit/Cost).
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Gets or sets the minimum value for this factor.
    /// </summary>
    public required string MinValue { get; set; }

    /// <summary>
    /// Gets or sets the maximum value for this factor.
    /// </summary>
    public required string MaxValue { get; set; }

    /// <summary>
    /// Gets or sets the transformation type (Linear, Log, Sigmoid).
    /// </summary>
    public string? Transform { get; set; }
}