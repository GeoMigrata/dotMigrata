using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Enums;
using dotGeoMigrata.Core.Values;
using dotGeoMigrata.Generator;

namespace dotGeoMigrata.Builder;

/// <summary>
/// Fluent builder for creating World instances with person-based populations.
/// Simplifies the process of creating cities, factors, and populating with individuals.
/// </summary>
public sealed class WorldBuilder
{
    private string _displayName = "Unnamed World";
    private readonly List<FactorDefinition> _factorDefinitions = [];
    private readonly List<City> _cities = [];

    /// <summary>
    /// Sets the display name for the world.
    /// </summary>
    /// <param name="name">The display name.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public WorldBuilder WithName(string name)
    {
        _displayName = name ?? throw new ArgumentNullException(nameof(name));
        return this;
    }

    /// <summary>
    /// Adds a factor definition to the world.
    /// </summary>
    /// <param name="displayName">Display name of the factor.</param>
    /// <param name="type">Factor type (Positive or Negative).</param>
    /// <param name="minValue">Minimum value for normalization.</param>
    /// <param name="maxValue">Maximum value for normalization.</param>
    /// <param name="transform">Optional transformation type for normalization.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public WorldBuilder AddFactor(
        string displayName,
        FactorType type,
        double minValue,
        double maxValue,
        TransformType? transform = null)
    {
        var factor = new FactorDefinition
        {
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName)),
            Type = type,
            MinValue = minValue,
            MaxValue = maxValue,
            Transform = transform
        };

        _factorDefinitions.Add(factor);
        return this;
    }

    /// <summary>
    /// Adds a pre-configured factor definition to the world.
    /// </summary>
    /// <param name="factorDefinition">The factor definition to add.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public WorldBuilder AddFactor(FactorDefinition factorDefinition)
    {
        _factorDefinitions.Add(factorDefinition ?? throw new ArgumentNullException(nameof(factorDefinition)));
        return this;
    }

    /// <summary>
    /// Adds a city to the world with configuration.
    /// </summary>
    /// <param name="displayName">Display name of the city.</param>
    /// <param name="latitude">Latitude coordinate.</param>
    /// <param name="longitude">Longitude coordinate.</param>
    /// <param name="area">Area in square kilometers.</param>
    /// <param name="capacity">Optional maximum population capacity.</param>
    /// <param name="configureCity">Optional action to configure the city.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public WorldBuilder AddCity(
        string displayName,
        double latitude,
        double longitude,
        double area,
        int? capacity = null,
        Action<CityBuilder>? configureCity = null)
    {
        var builder = new CityBuilder(displayName, latitude, longitude, area, capacity, _factorDefinitions);
        configureCity?.Invoke(builder);
        var city = builder.Build();
        _cities.Add(city);
        return this;
    }

    /// <summary>
    /// Adds a pre-configured city to the world.
    /// </summary>
    /// <param name="city">The city to add.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public WorldBuilder AddCity(City city)
    {
        _cities.Add(city ?? throw new ArgumentNullException(nameof(city)));
        return this;
    }

    /// <summary>
    /// Populates the world with randomly generated persons distributed across cities.
    /// </summary>
    /// <param name="totalPopulation">Total number of persons to generate.</param>
    /// <param name="cityDistribution">Optional dictionary specifying population per city name. If null, distributes evenly.</param>
    /// <param name="config">Optional person generator configuration.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public WorldBuilder WithRandomPopulation(
        int totalPopulation,
        IDictionary<string, int>? cityDistribution = null,
        PersonGeneratorConfig? config = null)
    {
        var generator = new PersonGenerator(config);

        // Create distribution map
        Dictionary<City, int> distribution;
        if (cityDistribution != null)
        {
            distribution = new Dictionary<City, int>();
            foreach (var (cityName, count) in cityDistribution)
            {
                var city = _cities.FirstOrDefault(c => c.DisplayName == cityName);
                if (city == null)
                    throw new ArgumentException($"City '{cityName}' not found in world.", nameof(cityDistribution));
                distribution[city] = count;
            }
        }
        else
        {
            // Distribute evenly
            var perCity = totalPopulation / _cities.Count;
            var remainder = totalPopulation % _cities.Count;
            distribution = new Dictionary<City, int>();
            for (var i = 0; i < _cities.Count; i++)
            {
                distribution[_cities[i]] = perCity + (i < remainder ? 1 : 0);
            }
        }

        generator.GenerateAndDistributePersons(totalPopulation, _factorDefinitions, distribution);
        return this;
    }

    /// <summary>
    /// Builds and returns the configured World instance.
    /// </summary>
    /// <returns>A validated World instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    public World Build()
    {
        return new World(_cities, _factorDefinitions)
        {
            DisplayName = _displayName
        };
    }
}

/// <summary>
/// Helper builder for configuring city properties and initial population.
/// </summary>
public sealed class CityBuilder
{
    private readonly string _displayName;
    private readonly double _latitude;
    private readonly double _longitude;
    private readonly double _area;
    private readonly int? _capacity;
    private readonly List<FactorDefinition> _worldFactorDefinitions;
    private readonly Dictionary<string, double> _factorValues = new();
    private readonly List<Person> _persons = new();

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
    /// Generates and adds random persons to the city.
    /// </summary>
    /// <param name="count">Number of persons to generate.</param>
    /// <param name="config">Optional person generator configuration.</param>
    /// <param name="idPrefix">Optional ID prefix for generated persons.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CityBuilder WithRandomPersons(int count, PersonGeneratorConfig? config = null, string? idPrefix = null)
    {
        var generator = new PersonGenerator(config);
        var prefix = idPrefix ?? $"{_displayName}_P";
        var persons = generator.GeneratePersons(count, _worldFactorDefinitions, prefix);
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
            {
                throw new InvalidOperationException(
                    $"City '{_displayName}' is missing a value for factor '{factorDef.DisplayName}'.");
            }

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