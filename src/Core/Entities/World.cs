using dotGeoMigrata.Core.Values;

namespace dotGeoMigrata.Core.Entities;

/// <summary>
/// Represents the top-level simulation world containing cities and factor definitions.
/// </summary>
public class World
{
    private readonly List<City> _cities;
    private readonly List<FactorDefinition> _factorDefinitions;

    /// <summary>
    /// Initializes a new instance of the World class.
    /// </summary>
    /// <param name="cities">The cities in the world.</param>
    /// <param name="factorDefinitions">The factor definitions used in the world.</param>
    /// <exception cref="ArgumentNullException">Thrown when cities or factorDefinitions is null.</exception>
    /// <exception cref="ArgumentException">Thrown when cities or factorDefinitions is empty.</exception>
    public World(IEnumerable<City> cities, IEnumerable<FactorDefinition> factorDefinitions)
    {
        ArgumentNullException.ThrowIfNull(cities);
        ArgumentNullException.ThrowIfNull(factorDefinitions);

        _cities = cities.ToList();
        _factorDefinitions = factorDefinitions.ToList();

        if (_cities.Count == 0)
            throw new ArgumentException("World must contain at least one city.", nameof(cities));
        if (_factorDefinitions.Count == 0)
            throw new ArgumentException("World must contain at least one factor definition.",
                nameof(factorDefinitions));
    }

    /// <summary>
    /// Gets the display name of the world.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets the read-only list of cities in the world.
    /// </summary>
    public IReadOnlyList<City> Cities => _cities;

    /// <summary>
    /// Gets the read-only list of factor definitions used in the world.
    /// </summary>
    public IReadOnlyList<FactorDefinition> FactorDefinitions => _factorDefinitions;

    /// <summary>
    /// Gets the total population across all cities in the world.
    /// </summary>
    public int Population => _cities.Sum(c => c.Population);
}