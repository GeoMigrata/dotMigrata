using dotGeoMigrata.Core.Values;

namespace dotGeoMigrata.Core.Entities;

public class World
{
    private readonly List<City> _cities;
    private readonly List<FactorDefinition> _factorDefinitions;

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

    public required string DisplayName { get; init; }
    public IReadOnlyList<City> Cities => _cities;
    public IReadOnlyList<FactorDefinition> FactorDefinitions => _factorDefinitions;

    public int Population => _cities.Sum(c => c.Population);
}