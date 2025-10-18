using dotGeoMigrata.Core.Domain.Values;
using dotGeoMigrata.Interfaces;

namespace dotGeoMigrata.Core.Domain.Entities;

internal class City : IIdentifiable
{
    public string Id { get; init; }
    public string? DisplayName { get; init; }
    public double Area { get; set; }
    public (double, double) Position { get; init; }
    public List<FactorValue> FactorValues { get; init; }
    public List<PopulationGroup> PopulationGroups { get; init; }

    public City(string id,
        double area,
        (double, double) position,
        string? displayName = null,
        List<FactorValue>? factorValues = null,
        List<PopulationGroup>? populationGroups = null) =>
        (Id, Area, Position, DisplayName, FactorValues, PopulationGroups) = (
            !string.IsNullOrWhiteSpace(id)
                ? id
                : throw new ArgumentException("Id of City must be non-empty", nameof(id)),
            area > 0 ? area : throw new ArgumentException("Area must be greater than 0", nameof(area)),
            position,
            displayName,
            factorValues ?? [],
            populationGroups ?? []
        );

    public int Population => PopulationGroups.Sum(g => g.Count);
}