using dotGeoMigrata.Core.Domain.Values;
using dotGeoMigrata.Interfaces;

namespace dotGeoMigrata.Core.Domain.Entities;

internal class World : IIdentifiable
{
    public string Id { get; init; }
    public string? DisplayName { get; init; }
    public IReadOnlyList<City> Cities { get; }
    public IReadOnlyList<FactorDefinition> Factors { get; }
    public int Population => Cities.Sum(c => c.Population);

    public World(string id, IEnumerable<City> cities, IEnumerable<FactorDefinition> factorDefinitions,
        string? displayName = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id of World must be non-empty", nameof(id));
        Id = id;
        DisplayName = displayName;
        Cities = cities is List<City> cList ? cList : cities.ToList();
        Factors = factorDefinitions is List<FactorDefinition> fList ? fList : factorDefinitions.ToList();
    }
}