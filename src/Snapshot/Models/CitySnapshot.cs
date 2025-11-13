namespace dotGeoMigrata.Snapshot.Models;

/// <summary>
/// Snapshot of a city's state at a specific point in time.
/// </summary>
public sealed record CitySnapshot
{
    public required string DisplayName { get; init; }
    public required double Latitude { get; init; }
    public required double Longitude { get; init; }
    public required double Area { get; init; }
    public required int? Capacity { get; init; }
    public required Dictionary<string, double> FactorValues { get; init; }
    public required List<Guid> PersonIds { get; init; }
}