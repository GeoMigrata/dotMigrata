using dotMigrata.Core.Exceptions;
using dotMigrata.Core.Values;

namespace dotMigrata.Core.Entities;

/// <summary>
/// Represents the top-level simulation world containing cities, factor definitions, and individual persons.
/// </summary>
public class World
{
    private readonly List<City> _cities;
    private readonly List<FactorDefinition> _factorDefinitions;

    /// <summary>
    /// Initializes a new instance of the <see cref="World" /> class.
    /// </summary>
    /// <param name="cities">The cities in the world.</param>
    /// <param name="factorDefinitions">The factor definitions used in the world.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="cities" /> or <paramref name="factorDefinitions" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="cities" /> or <paramref name="factorDefinitions" /> is empty.
    /// </exception>
    public World(
        IEnumerable<City> cities,
        IEnumerable<FactorDefinition> factorDefinitions)
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

        ValidateWorldStructure();
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
    /// Gets all persons across all cities in the world.
    /// </summary>
    public IEnumerable<PersonBase> AllPersons => _cities.SelectMany(c => c.Persons);

    /// <summary>
    /// Gets the total population across all cities in the world.
    /// </summary>
    public int Population => _cities.Sum(c => c.Population);

    /// <summary>
    /// Validates that each city has values for all factor definitions.
    /// Validates that all persons in the world are of the same type.
    /// </summary>
    /// <exception cref="WorldValidationException">
    /// Thrown when a city is missing values for one or more factor definitions,
    /// or when persons of different types are mixed in the world.
    /// </exception>
    private void ValidateWorldStructure()
    {
        // Validate that each city has values for all factor definitions
        foreach (var city in _cities)
        {
            var cityFactors = city.FactorIntensities.Select(fi => fi.Definition).ToHashSet();
            var missingFactors = _factorDefinitions.Where(fd => !cityFactors.Contains(fd)).ToList();

            if (missingFactors.Count > 0)
                throw new WorldValidationException(
                    city.DisplayName,
                    missingFactors.Select(f => f.DisplayName));
        }

        // Validate that all persons are of the same concrete type
        Type? personType = null;
        foreach (var currentType in from city in _cities from person in city.Persons select person.GetType())
            if (personType == null)
                personType = currentType;
            else if (personType != currentType)
                throw new WorldValidationException(
                    $"World contains mixed person types: {personType.Name} and {currentType.Name}. " +
                    $"A simulation world must contain only one person type.");
    }
}