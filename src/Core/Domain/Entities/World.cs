using dotGeoMigrata.Core.Domain.Values;

namespace dotGeoMigrata.Core.Domain.Entities;

public class World
{
    public required string DisplayName { get; init; }

    private readonly List<City> _cities;
    public IReadOnlyList<City> Cities => _cities;
    private readonly List<FactorDefinition> _factorDefinitions;
    public IReadOnlyList<FactorDefinition> FactorDefinitions => _factorDefinitions;

    public int Population => _cities.Sum(c => c.Population);

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
}