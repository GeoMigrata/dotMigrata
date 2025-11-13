namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Snapshot of a person's state at a specific point in time.
/// </summary>
public sealed record PersonSnapshot
{
    public required Guid Id { get; init; }
    public required string? CurrentCityName { get; init; }
    public required double MovingWillingness { get; init; }
    public required double RetentionRate { get; init; }
    public required double SensitivityScaling { get; init; }
    public required double AttractionThreshold { get; init; }
    public required double MinimumAcceptableAttraction { get; init; }
    public required Dictionary<string, double> FactorSensitivities { get; init; }
    public required List<string> Tags { get; init; }
}