namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Snapshot of a factor definition.
/// </summary>
public sealed record FactorSnapshot
{
    public required string DisplayName { get; init; }
    public required string Type { get; init; }
    public required double MinValue { get; init; }
    public required double MaxValue { get; init; }
    public required string? Transform { get; init; }
}