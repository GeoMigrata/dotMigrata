using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Values;
using dotGeoMigrata.Generator;

namespace dotGeoMigrata.Builder;

/// <summary>
/// Helper builder for configuring city properties and initial population.
/// </summary>
public sealed class CityBuilder
{
    private readonly double _area;
    private readonly int? _capacity;
    private readonly string _displayName;
    private readonly Dictionary<string, double> _factorValues = new();
    private readonly double _latitude;
    private readonly double _longitude;
    private readonly List<Person> _persons = new();
    private readonly List<FactorDefinition> _worldFactorDefinitions;

    internal CityBuilder(
        string displayName,
        double latitude,
        double longitude,
        double area,
        int? capacity,
        List<FactorDefinition> worldFactorDefinitions)
    {
        _displayName = displayName;
        _latitude = latitude;
        _longitude = longitude;
        _area = area;
        _capacity = capacity;
        _worldFactorDefinitions = worldFactorDefinitions;
    }

    /// <summary>
    /// Sets the intensity value for a specific factor.
    /// </summary>
    /// <param name="factorName">The name of the factor.</param>
    /// <param name="intensity">The intensity value.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CityBuilder WithFactorValue(string factorName, double intensity)
    {
        _factorValues[factorName] = intensity;
        return this;
    }

    /// <summary>
    /// Sets the intensity value for a specific factor using a factor definition reference.
    /// </summary>
    /// <param name="factorDefinition">The factor definition.</param>
    /// <param name="intensity">The intensity value.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CityBuilder WithFactorValue(FactorDefinition factorDefinition, double intensity)
    {
        ArgumentNullException.ThrowIfNull(factorDefinition);
        _factorValues[factorDefinition.DisplayName] = intensity;
        return this;
    }

    /// <summary>
    /// Adds a person to the city.
    /// </summary>
    /// <param name="person">The person to add.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CityBuilder WithPerson(Person person)
    {
        _persons.Add(person ?? throw new ArgumentNullException(nameof(person)));
        return this;
    }

    /// <summary>
    /// Adds multiple persons to the city.
    /// </summary>
    /// <param name="persons">The persons to add.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CityBuilder WithPersons(IEnumerable<Person> persons)
    {
        ArgumentNullException.ThrowIfNull(persons);
        _persons.AddRange(persons);
        return this;
    }

    /// <summary>
    /// Generates and adds random persons to the city using a PersonCollection.
    /// </summary>
    /// <param name="collection">The person collection containing specifications.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CityBuilder WithPersonCollection(PersonCollection collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        var persons = collection.GenerateAllPersons(_worldFactorDefinitions);
        _persons.AddRange(persons);
        return this;
    }

    internal City Build()
    {
        // Create factor values
        var factorValues = new List<FactorValue>();
        foreach (var factorDef in _worldFactorDefinitions)
        {
            if (!_factorValues.TryGetValue(factorDef.DisplayName, out var intensity))
                throw new InvalidOperationException(
                    $"City '{_displayName}' is missing a value for factor '{factorDef.DisplayName}'.");

            factorValues.Add(new FactorValue
            {
                Definition = factorDef,
                Intensity = intensity
            });
        }

        // Create city
        var city = new City(factorValues, _persons)
        {
            DisplayName = _displayName,
            Location = new Coordinate { Latitude = _latitude, Longitude = _longitude },
            Area = _area,
            Capacity = _capacity
        };

        return city;
    }
}