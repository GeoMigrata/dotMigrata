namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Snapshot of a person's state at a specific point in time.
/// Uses temporary index-based ID for serialization only.
/// </summary>
public sealed record PersonSnapshot
{
    /// <summary>
    /// Temporary index-based ID used only during serialization/deserialization.
    /// Not stored in the actual Person object.
    /// </summary>
    public required int Index { get; init; }

    public string? CurrentCityName { get; init; }
    public required double MovingWillingness { get; init; }
    public required double RetentionRate { get; init; }
    public required double SensitivityScaling { get; init; }
    public required double AttractionThreshold { get; init; }
    public required double MinimumAcceptableAttraction { get; init; }
    public required Dictionary<string, double> FactorSensitivities { get; init; }
    public required List<string> Tags { get; init; }
}