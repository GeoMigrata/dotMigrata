using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Enums;
using dotGeoMigrata.Core.Values;

namespace dotGeoMigrata.Builder;

/// <summary>
/// Fluent builder for creating World instances with proper validation.
/// Simplifies the process of creating cities, factors, and population groups.
/// </summary>
public sealed class WorldBuilder
{
    private string _displayName = "Unnamed World";
    private readonly List<FactorDefinition> _factorDefinitions = [];
    private readonly List<GroupDefinition> _groupDefinitions = [];
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
    /// Adds a population group definition to the world.
    /// </summary>
    /// <param name="displayName">Display name of the group.</param>
    /// <param name="movingWillingness">Willingness to migrate (0-1).</param>
    /// <param name="retentionRate">Retention rate (0-1).</param>
    /// <param name="configureSensitivities">Action to configure factor sensitivities.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public WorldBuilder AddPopulationGroup(
        string displayName,
        double movingWillingness,
        double retentionRate,
        Action<GroupDefinitionBuilder> configureSensitivities)
    {
        var builder = new GroupDefinitionBuilder(displayName, movingWillingness, retentionRate);
        configureSensitivities(builder);
        var groupDefinition = builder.Build(_factorDefinitions);
        _groupDefinitions.Add(groupDefinition);
        return this;
    }

    /// <summary>
    /// Adds a pre-configured population group definition to the world.
    /// </summary>
    /// <param name="groupDefinition">The group definition to add.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public WorldBuilder AddPopulationGroup(GroupDefinition groupDefinition)
    {
        _groupDefinitions.Add(groupDefinition ?? throw new ArgumentNullException(nameof(groupDefinition)));
        return this;
    }

    /// <summary>
    /// Adds a city to the world.
    /// </summary>
    /// <param name="displayName">Display name of the city.</param>
    /// <param name="latitude">Latitude coordinate.</param>
    /// <param name="longitude">Longitude coordinate.</param>
    /// <param name="area">Area in square kilometers.</param>
    /// <param name="capacity">Optional maximum population capacity.</param>
    /// <param name="configureCity">Action to configure city factors and population.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public WorldBuilder AddCity(
        string displayName,
        double latitude,
        double longitude,
        double area,
        int? capacity,
        Action<CityBuilder> configureCity)
    {
        var builder = new CityBuilder(displayName, latitude, longitude, area, capacity);
        configureCity(builder);
        var city = builder.Build(_factorDefinitions, _groupDefinitions);
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
    /// Builds and returns the configured World instance.
    /// </summary>
    /// <returns>A validated World instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    public World Build()
    {
        return new World(_cities, _factorDefinitions, _groupDefinitions)
        {
            DisplayName = _displayName
        };
    }
}

/// <summary>
/// Helper builder for configuring population group sensitivities.
/// </summary>
public sealed class GroupDefinitionBuilder
{
    private readonly string _displayName;
    private readonly double _movingWillingness;
    private readonly double _retentionRate;
    private readonly List<(string FactorName, int Sensitivity, FactorType? Override)> _sensitivities = new();
    private double _sensitivityScaling = 1.0;
    private double _attractionThreshold;
    private double _minimumAcceptableAttraction;

    internal GroupDefinitionBuilder(string displayName, double movingWillingness, double retentionRate)
    {
        _displayName = displayName;
        _movingWillingness = movingWillingness;
        _retentionRate = retentionRate;
    }

    /// <summary>
    /// Adds a factor sensitivity for this population group.
    /// </summary>
    /// <param name="factorName">Display name of the factor.</param>
    /// <param name="sensitivity">Sensitivity weight.</param>
    /// <param name="overrideType">Optional factor type override for this group.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GroupDefinitionBuilder WithSensitivity(string factorName, int sensitivity, FactorType? overrideType = null)
    {
        _sensitivities.Add((factorName, sensitivity, overrideType));
        return this;
    }

    /// <summary>
    /// Sets the sensitivity scaling coefficient.
    /// </summary>
    /// <param name="scaling">Sensitivity scaling value.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GroupDefinitionBuilder WithSensitivityScaling(double scaling)
    {
        _sensitivityScaling = scaling;
        return this;
    }

    /// <summary>
    /// Sets the attraction threshold.
    /// </summary>
    /// <param name="threshold">Attraction threshold value.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GroupDefinitionBuilder WithAttractionThreshold(double threshold)
    {
        _attractionThreshold = threshold;
        return this;
    }

    /// <summary>
    /// Sets the minimum acceptable attraction score.
    /// </summary>
    /// <param name="minimum">Minimum acceptable attraction value.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public GroupDefinitionBuilder WithMinimumAcceptableAttraction(double minimum)
    {
        _minimumAcceptableAttraction = minimum;
        return this;
    }

    internal GroupDefinition Build(List<FactorDefinition> availableFactors)
    {
        var factorSensitivities = _sensitivities.Select(s =>
        {
            var factor = availableFactors.FirstOrDefault(f => f.DisplayName == s.FactorName)
                         ?? throw new InvalidOperationException($"Factor '{s.FactorName}' not found in world.");
            return new FactorSensitivity
            {
                Factor = factor,
                Sensitivity = s.Sensitivity,
                OverriddenFactorType = s.Override
            };
        }).ToList();

        return new GroupDefinition(factorSensitivities)
        {
            DisplayName = _displayName,
            MovingWillingness = _movingWillingness,
            RetentionRate = _retentionRate,
            SensitivityScaling = _sensitivityScaling,
            AttractionThreshold = _attractionThreshold,
            MinimumAcceptableAttraction = _minimumAcceptableAttraction
        };
    }
}

/// <summary>
/// Helper builder for configuring cities.
/// </summary>
public sealed class CityBuilder
{
    private readonly string _displayName;
    private readonly double _latitude;
    private readonly double _longitude;
    private readonly double _area;
    private readonly int? _capacity;
    private readonly Dictionary<string, double> _factorIntensities = new();
    private readonly Dictionary<string, int> _populationCounts = new();

    internal CityBuilder(string displayName, double latitude, double longitude, double area, int? capacity)
    {
        _displayName = displayName;
        _latitude = latitude;
        _longitude = longitude;
        _area = area;
        _capacity = capacity;
    }

    /// <summary>
    /// Sets the intensity value for a factor.
    /// </summary>
    /// <param name="factorName">Display name of the factor.</param>
    /// <param name="intensity">Intensity value.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CityBuilder WithFactorValue(string factorName, double intensity)
    {
        _factorIntensities[factorName] = intensity;
        return this;
    }

    /// <summary>
    /// Sets the population count for a population group.
    /// </summary>
    /// <param name="groupName">Display name of the population group.</param>
    /// <param name="population">Population count.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public CityBuilder WithPopulation(string groupName, int population)
    {
        _populationCounts[groupName] = population;
        return this;
    }

    internal City Build(List<FactorDefinition> availableFactors, List<GroupDefinition> availableGroups)
    {
        // Create factor values for all factors
        var factorValues = availableFactors.Select(fd => new FactorValue
        {
            Definition = fd,
            Intensity = _factorIntensities.TryGetValue(fd.DisplayName, out var intensity)
                ? intensity
                : throw new InvalidOperationException(
                    $"City '{_displayName}' is missing intensity value for factor '{fd.DisplayName}'.")
        }).ToList();

        // Create population group values for all groups
        var groupValues = availableGroups.Select(gd => new GroupValue
        {
            Definition = gd,
            Population = _populationCounts.GetValueOrDefault(gd.DisplayName, 0) // Default to 0 if not specified
        }).ToList();

        return new City(factorValues, groupValues)
        {
            DisplayName = _displayName,
            Location = new Coordinate
            {
                Latitude = _latitude,
                Longitude = _longitude
            },
            Area = _area,
            Capacity = _capacity
        };
    }
}