using dotGeoMigrata.Core.Values;

namespace dotGeoMigrata.Core.Entities;

/// <summary>
/// Represents a city with geographic location, factors, and population group values.
/// </summary>
public class City
{
    private readonly double _area;

    private readonly Dictionary<FactorDefinition, FactorValue> _factorLookup;

    private readonly List<FactorValue> _factorValues;

    private readonly Dictionary<GroupDefinition, GroupValue> _populationGroupLookup;

    private readonly List<GroupValue> _populationGroupValues;

    /// <summary>
    /// Initializes a new instance of the City class.
    /// </summary>
    /// <param name="factorValues">Optional initial factor values.</param>
    /// <param name="populationGroupValues">Optional initial population group values.</param>
    public City(
        IEnumerable<FactorValue>? factorValues = null,
        IEnumerable<GroupValue>? populationGroupValues = null)
    {
        _factorValues = factorValues?.ToList() ?? [];
        _populationGroupValues = populationGroupValues?.ToList() ?? [];

        _factorLookup = _factorValues.ToDictionary(fv => fv.Definition, fv => fv);
        _populationGroupLookup = _populationGroupValues.ToDictionary(pgv => pgv.Definition, pgv => pgv);
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
    /// Gets or initializes the maximum population capacity of the city.
    /// Represents the upper limit of residents the city can accommodate.
    /// A value of null or 0 indicates no explicit capacity limit.
    /// </summary>
    public int? Capacity { get; init; }

    /// <summary>
    /// Gets the read-only list of factor values for this city.
    /// </summary>
    public IReadOnlyList<FactorValue> FactorValues => _factorValues;

    /// <summary>
    /// Gets the read-only list of population group values in this city.
    /// </summary>
    public IReadOnlyList<GroupValue> PopulationGroupValues => _populationGroupValues;

    /// <summary>
    /// Gets the total population of all groups in this city.
    /// </summary>
    public int Population => _populationGroupValues.Sum(pgv => pgv.Population);

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
    /// Tries to get the population group value for the specified population group definition.
    /// </summary>
    /// <param name="definition">The population group definition to look up.</param>
    /// <param name="groupValue">The population group value if found.</param>
    /// <returns>True if the population group value exists, false otherwise.</returns>
    public bool TryGetPopulationGroupValue(GroupDefinition definition, out GroupValue? groupValue)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return _populationGroupLookup.TryGetValue(definition, out groupValue);
    }

    /// <summary>
    /// Updates the population count for a specific population group definition.
    /// </summary>
    /// <param name="definition">The population group definition.</param>
    /// <param name="newCount">The new population count.</param>
    /// <exception cref="ArgumentNullException">Thrown when definition is null.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the definition has no matched value in this city or when newCount is
    /// negative.
    /// </exception>
    public void UpdatePopulationCount(GroupDefinition definition, int newCount)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (newCount < 0)
            throw new ArgumentException("Population count cannot be negative.", nameof(newCount));

        if (!_populationGroupLookup.TryGetValue(definition, out var groupValue))
            throw new ArgumentException("Given population group definition has no matched value in this city.",
                nameof(definition));

        groupValue.Population = newCount;
    }
}