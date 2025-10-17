using dotGeoMigrata.Core.Domain.Values;

namespace dotGeoMigrata.Core.Domain.Entities;

internal class City
{
    public required string Id { get; init; }
    public required string DisplayName { get; init; }
    public required double Area { get; set; }
    public required (double, double) Position { get; init; }
    public required List<FactorValue> FactorValues { get; init; }
    public required List<PopulationGroup> PopulationGroups { get; init; }

    public City(string id,
        double area,
        (double, double) position,
        string? displayName = null,
        List<FactorValue>? factorValues = null,
        List<PopulationGroup>? populationGroups = null) =>
        (Id, Area, Position, FactorValues, PopulationGroups) = (
            !string.IsNullOrWhiteSpace(id)
                ? id
                : throw new ArgumentException("Id of City must be non-empty", nameof(id)),
            area > 0 ? area : throw new ArgumentException("Area must be greater than 0", nameof(area)),
            position,
            factorValues ?? [],
            populationGroups ?? []
        );

    public int Population => PopulationGroups.Sum(g => g.Count);
}