using System.Runtime.CompilerServices;
using dotGeoMigrata.Core.Domain.Values;

namespace dotGeoMigrata.Core.Domain.Entities;

public class City(
    string displayName,
    double area,
    Coordinate position,
    IEnumerable<FactorValue>? factorValues = null,
    IEnumerable<PopulationGroup>? populationGroups = null)
{
    public string DisplayName { get; init; } = displayName;

    public double Area { get; set; } =
        area > 0 ? area : throw new ArgumentException("Area must be greater than 0", nameof(area));

    public Coordinate Position { get; init; } = position;

    private readonly List<FactorValue> _factorValues = factorValues?.ToList() ?? [];
    public IReadOnlyList<FactorValue> FactorValues => _factorValues;

    private readonly List<PopulationGroup> _populationGroups = populationGroups?.ToList() ?? [];
    public IReadOnlyList<PopulationGroup> PopulationGroups => _populationGroups;

    public int Population => _populationGroups.Sum(g => g.Count);

    /// <summary>
    /// Replace or an existing FactorValue for the specified factor definition.
    /// This keeps FactorValue immutable but allows controlled updates via City API.
    /// </summary>
    public void UpdateFactorIntensity(FactorDefinition factor, double newIntensity)
    {
        var item = _factorValues.FirstOrDefault(fv => fv.Factor == factor);
        if (item == null)
            throw new ArgumentException("Given factor has no matched value in this city.", nameof(factor));

        item.Intensity = newIntensity;
    }

    public void AddPopulationGroup(PopulationGroup group) => _populationGroups.Add(group);

    public void RemovePopulationGroup(PopulationGroup group) => _populationGroups.Remove(group);
}