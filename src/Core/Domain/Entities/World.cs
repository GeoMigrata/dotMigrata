using dotGeoMigrata.Core.Domain.Values;

namespace dotGeoMigrata.Core.Domain.Entities;

internal static class World
{
    public static HashSet<City> Cities { get; } = [];
    public static int TotalPopulation => Cities.Sum(c => c.Population);
    public static HashSet<FactorDefinition> Factors { get; } = [];

    public static void AddCity(City city) => Cities.Add(city);
    public static void RemoveCity(City city) => Cities.Remove(city);

    public static void AddFactor(FactorDefinition factor) => Factors.Add(factor);
    public static void RemoveFactor(FactorDefinition factor) => Factors.Remove(factor);
}