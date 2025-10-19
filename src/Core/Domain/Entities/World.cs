using dotGeoMigrata.Core.Domain.Values;
using dotGeoMigrata.Interfaces;

namespace dotGeoMigrata.Core.Domain.Entities;

internal class World : IIdentifiable
{
    public string DisplayName { get; init; }

    private readonly List<City> _cities;
    public IReadOnlyList<City> Cities => _cities;
    private readonly List<FactorDefinition> _factorDefinitions;
    public IReadOnlyList<FactorDefinition> FactorDefinitions => _factorDefinitions;

    public int Population => _cities.Sum(c => c.Population);

    public World(string displayName, IEnumerable<City> cities, IEnumerable<FactorDefinition> factorDefinitions)
    {
        DisplayName = displayName;
        _cities = cities is List<City> cList ? cList : cities.ToList();
        _factorDefinitions = factorDefinitions is List<FactorDefinition> fList ? fList : factorDefinitions.ToList();
    }
}