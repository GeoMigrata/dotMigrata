using dotGeoMigrata.Core.Domain.Values;

namespace dotGeoMigrata.Core.Domain.Entities;

internal class World
{
    public IReadOnlyList<City> Cities { get; }
    public IReadOnlyList<FactorDefinition> Factors { get; }
    public int Population => Cities.Sum(c => c.Population);

    public World(IEnumerable<City> cities, IEnumerable<FactorDefinition> factorDefinitions)
    {
        Cities = cities is List<City> cList ? cList : cities.ToList();
        Factors = factorDefinitions is List<FactorDefinition> fList ? fList : factorDefinitions.ToList();
    }
}