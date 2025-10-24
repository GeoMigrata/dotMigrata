using dotGeoMigrata.Core.Domain.Values;

namespace dotGeoMigrata.Core.Domain.Entities;

public class City
{
    public required string DisplayName { get; init; }
    private readonly double _area;

    public double Area
    {
        get => _area;
        init => _area = value > 0 ? value : throw new ArgumentException("Area must be greater than 0", nameof(value));
    }

    public required Coordinate Position { get; init; }

    private readonly List<FactorValue> _factorValues;
    public IReadOnlyList<FactorValue> FactorValues => _factorValues;

    private readonly List<PopulationGroup> _populationGroups;

    public IReadOnlyList<PopulationGroup> PopulationGroups => _populationGroups;

    private readonly Dictionary<FactorDefinition, FactorValue> _factorLookup;

    public City(IEnumerable<FactorValue>? factorValues = null, IEnumerable<PopulationGroup>? populationGroups = null)
    {
        _factorValues = factorValues?.ToList() ?? [];
        _populationGroups = populationGroups?.ToList() ?? [];
        _factorLookup = _factorValues.ToDictionary(fv => fv.Factor, fv => fv);
    }

    public int Population => _populationGroups.Sum(g => g.Count);

    /// <summary>
    /// Updates the intensity of an existing FactorValue for the specified factor definition.
    /// This keeps FactorValue immutable but allows controlled updates via City API.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when factor is null.</exception>
    /// <exception cref="ArgumentException">Thrown when factor has no matched value in this city.</exception>
    public void UpdateFactorIntensity(FactorDefinition factor, double newIntensity)
    {
        ArgumentNullException.ThrowIfNull(factor);

        if (!_factorLookup.TryGetValue(factor, out var factorValue))
            throw new ArgumentException("Given factor has no matched value in this city.", nameof(factor));

        factorValue.Intensity = newIntensity;
    }

    /// <summary>
    /// Tries to get the factor value for the specified factor definition.
    /// </summary>
    /// <param name="factor">The factor definition to look up.</param>
    /// <param name="factorValue">The factor value if found.</param>
    /// <returns>True if the factor value exists, false otherwise.</returns>
    public bool TryGetFactorValue(FactorDefinition factor, out FactorValue? factorValue)
    {
        ArgumentNullException.ThrowIfNull(factor);
        return _factorLookup.TryGetValue(factor, out factorValue);
    }

    /// <summary>
    /// Adds a population group to this city.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when group is null.</exception>
    public void AddPopulationGroup(PopulationGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        _populationGroups.Add(group);
    }

    /// <summary>
    /// Removes a population group from this city.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when group is null.</exception>
    public void RemovePopulationGroup(PopulationGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        _populationGroups.Remove(group);
    }
}