using dotGeoMigrata.Core.Values;

namespace dotGeoMigrata.Core.Entities;

/// <summary>
/// Represents a city with geographic location, factors, and population groups.
/// </summary>
public class City
{
    private readonly double _area;

    private readonly Dictionary<FactorDefinition, FactorValue> _factorLookup;

    private readonly List<FactorValue> _factorValues;

    private readonly List<PopulationGroup> _populationGroups;

    /// <summary>
    /// Initializes a new instance of the City class.
    /// </summary>
    /// <param name="factorValues">Optional initial factor values.</param>
    /// <param name="populationGroups">Optional initial population groups.</param>
    public City(IEnumerable<FactorValue>? factorValues = null, IEnumerable<PopulationGroup>? populationGroups = null)
    {
        _factorValues = factorValues?.ToList() ?? [];
        _populationGroups = populationGroups?.ToList() ?? [];
        _factorLookup = _factorValues.ToDictionary(fv => fv.Factor, fv => fv);
    }

    /// <summary>
    /// Gets or initializes the display name of the city.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets or initializes the area of the city in square kilometers.
    /// Must be greater than 0.
    /// </summary>
    public double Area
    {
        get => _area;
        init => _area = value > 0 ? value : throw new ArgumentException("Area must be greater than 0", nameof(value));
    }

    /// <summary>
    /// Gets or initializes the geographic position of the city.
    /// </summary>
    public required Coordinate Location { get; init; }

    /// <summary>
    /// Gets the read-only list of factor values for this city.
    /// </summary>
    public IReadOnlyList<FactorValue> FactorValues => _factorValues;

    /// <summary>
    /// Gets the read-only list of population groups in this city.
    /// </summary>
    public IReadOnlyList<PopulationGroup> PopulationGroups => _populationGroups;

    /// <summary>
    /// Gets the total population of all groups in this city.
    /// </summary>
    public int Population => _populationGroups.Sum(g => g.Count);

    /// <summary>
    ///     Updates the intensity of an existing FactorValue for the specified factor definition.
    ///     This keeps FactorValue immutable but allows controlled updates via City API.
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
    ///     Tries to get the factor value for the specified factor definition.
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
    ///     Adds a population group to this city.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when group is null.</exception>
    public void AddPopulationGroup(PopulationGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        _populationGroups.Add(group);
    }

    /// <summary>
    ///     Removes a population group from this city.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when group is null.</exception>
    public void RemovePopulationGroup(PopulationGroup group)
    {
        ArgumentNullException.ThrowIfNull(group);
        _populationGroups.Remove(group);
    }
}