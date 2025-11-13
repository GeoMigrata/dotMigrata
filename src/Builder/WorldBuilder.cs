using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Enums;
using dotGeoMigrata.Core.Values;

namespace dotGeoMigrata.Builder;

/// <summary>
/// Fluent builder for creating World instances with person-based populations.
/// Simplifies the process of creating cities, factors, and populating with individuals.
/// </summary>
public sealed class WorldBuilder
{
    private readonly List<City> _cities = [];
    private readonly List<FactorDefinition> _factorDefinitions = [];
    private string _displayName = "Unnamed World";

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