using dotGeoMigrata.Core.Values;

namespace dotGeoMigrata.Core.Entities;

/// <summary>
/// Represents the top-level simulation world containing cities, factor definitions, and population group definitions.
/// </summary>
public class World
{
    private readonly List<City> _cities;
    private readonly List<FactorDefinition> _factorDefinitions;
    private readonly List<GroupDefinition> _populationGroupDefinitions;

    /// <summary>
    /// Initializes a new instance of the World class.
    /// </summary>
    /// <param name="cities">The cities in the world.</param>
    /// <param name="factorDefinitions">The factor definitions used in the world.</param>
    /// <param name="populationGroupDefinitions">The population group definitions used in the world.</param>
    /// <exception cref="ArgumentNullException">Thrown when cities, factorDefinitions, or populationGroupDefinitions is null.</exception>
    /// <exception cref="ArgumentException">Thrown when cities, factorDefinitions, or populationGroupDefinitions is empty.</exception>
    public World(
        IEnumerable<City> cities,
        IEnumerable<FactorDefinition> factorDefinitions,
        IEnumerable<GroupDefinition> populationGroupDefinitions)
    {
        ArgumentNullException.ThrowIfNull(cities);
        ArgumentNullException.ThrowIfNull(factorDefinitions);
        ArgumentNullException.ThrowIfNull(populationGroupDefinitions);

        _cities = cities.ToList();
        _factorDefinitions = factorDefinitions.ToList();
        _populationGroupDefinitions = populationGroupDefinitions.ToList();

        if (_cities.Count == 0)
            throw new ArgumentException("World must contain at least one city.", nameof(cities));
        if (_factorDefinitions.Count == 0)
            throw new ArgumentException("World must contain at least one factor definition.",
                nameof(factorDefinitions));
        if (_populationGroupDefinitions.Count == 0)
            throw new ArgumentException("World must contain at least one population group definition.",
                nameof(populationGroupDefinitions));

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
    /// Gets the read-only list of population group definitions used in the world.
    /// </summary>
    public IReadOnlyList<GroupDefinition> PopulationGroupDefinitions => _populationGroupDefinitions;

    /// <summary>
    /// Gets the total population across all cities in the world.
    /// </summary>
    public int Population => _cities.Sum(c => c.Population);

    /// <summary>
    /// Validates that each city has values for all factor and population group definitions.
    /// Also validates that each population group definition has sensitivities for all factor definitions.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    private void ValidateWorldStructure()
    {
        // Validate that each population group definition has sensitivities for all factor definitions
        foreach (var populationGroupDef in _populationGroupDefinitions)
        {
            var sensitivityFactors = populationGroupDef.Sensitivities
                .Select(s => s.Factor)
                .ToHashSet();

            var missingFactors = _factorDefinitions
                .Where(fd => !sensitivityFactors.Contains(fd))
                .ToList();

            if (missingFactors.Count <= 0) continue;
            var missingNames = string.Join(", ", missingFactors.Select(f => f.DisplayName));
            throw new InvalidOperationException(
                $"Population group '{populationGroupDef.DisplayName}' is missing sensitivities for factors: {missingNames}");
        }

        // Validate that each city has values for all factor definitions
        foreach (var city in _cities)
        {
            var cityFactors = city.FactorValues.Select(fv => fv.Definition).ToHashSet();
            var missingFactors = _factorDefinitions
                .Where(fd => !cityFactors.Contains(fd))
                .ToList();

            if (missingFactors.Count > 0)
            {
                var missingNames = string.Join(", ", missingFactors.Select(f => f.DisplayName));
                throw new InvalidOperationException(
                    $"City '{city.DisplayName}' is missing values for factors: {missingNames}");
            }

            // Validate that each city has values for all population group definitions
            var cityPopulationGroups = city.PopulationGroupValues.Select(pgv => pgv.Definition).ToHashSet();
            var missingGroups = _populationGroupDefinitions
                .Where(pgd => !cityPopulationGroups.Contains(pgd))
                .ToList();

            if (missingGroups.Count <= 0) continue;
            {
                var missingNames = string.Join(", ", missingGroups.Select(g => g.DisplayName));
                throw new InvalidOperationException(
                    $"City '{city.DisplayName}' is missing values for population groups: {missingNames}");
            }
        }
    }
}