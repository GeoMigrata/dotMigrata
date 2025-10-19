using dotGeoMigrata.Core.Domain.Values;
using dotGeoMigrata.Interfaces;

namespace dotGeoMigrata.Core.Domain.Entities;

internal class City : IIdentifiable
{
    public string DisplayName { get; init; }
    public double Area { get; set; }
    public Coordinate Position { get; init; }

    private readonly List<FactorValue> _factorValues;
    public IReadOnlyList<FactorValue> FactorValues => _factorValues;

    private readonly List<PopulationGroup> _populationGroups;
    public IReadOnlyList<PopulationGroup> PopulationGroups => _populationGroups;

    public City(
        string displayName,
        double area,
        Coordinate position,
        IEnumerable<FactorValue>? factorValues = null,
        IEnumerable<PopulationGroup>? populationGroups = null) =>
        (Area, Position, DisplayName, _factorValues, _populationGroups) = (
            area > 0 ? area : throw new ArgumentException("Area must be greater than 0", nameof(area)),
            position,
            displayName,
            factorValues?.ToList() ?? [],
            populationGroups?.ToList() ?? []
        );

    public int Population => _populationGroups.Sum(g => g.Count);
}