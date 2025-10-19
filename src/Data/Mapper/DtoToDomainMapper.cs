using System.Data;
using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Core.Domain.Enums;
using dotGeoMigrata.Core.Domain.Values;
using dotGeoMigrata.Data.DTO;

namespace dotGeoMigrata.Data.Mapper;

internal static class DtoToDomainMapper
{
    public static World ToDomain(WorldDto dto)
    {
        var factorDefinitions = dto.FactorDefinitions
            .ToDictionary(p => p.Key, p => new FactorDefinition(
                p.Value.DisplayName,
                p.Value.Type,
                p.Value.MinValue,
                p.Value.MaxValue,
                p.Value.Transform
            ));

        var cities = dto.Cities
            .Select(p => CreateCity(p.Key, p.Value, factorDefinitions))
            .ToList();

        return new World(
            displayName: dto.DisplayName,
            cities: cities,
            factorDefinitions: factorDefinitions.Values);
    }

    private static City CreateCity(string id, CityDto dto, Dictionary<string, FactorDefinition> factorDefinitions)
    {
        var factorValues = dto.FactorValues.Select(kv => new FactorValue(factorDefinitions[kv.Key], kv.Value));
        var populationGroups =
            dto.PopulationGroups.Select(kv => CreateGroup(kv.Key, kv.Value, factorDefinitions));

        return new City(
            displayName: dto.DisplayName,
            area: dto.Area,
            position: dto.Position,
            factorValues: factorValues,
            populationGroups: populationGroups
        );
    }

    private static PopulationGroup CreateGroup(string id, PopulationGroupDto dto,
        Dictionary<string, FactorDefinition> factorDefinitions)
    {
        var sensitivities = dto.FactorSensitivities.Select(kv =>
        {
            var factorId = kv.Key;
            var sensDto = kv.Value;

            if (!factorDefinitions.TryGetValue(factorId, out var factorDefinition))
                throw new KeyNotFoundException(
                    $"FactorDefinition '{factorId}' not found when building PopulationGroup '{id}'.");

            var overriddenType = sensDto.OverriddenFactorType;

            return new FactorSensitivity(
                factor: factorDefinition,
                overriddenFactorType: sensDto.OverriddenFactorType,
                sensitivity: sensDto.Value);
        });

        return new PopulationGroup(
            displayName: dto.DisplayName,
            count: dto.Count,
            movingWillingness: dto.MovingWillingness,
            maxMigrationThreshold: dto.MaxMigrationThreshold,
            minMigrationThreshold: dto.MinAcceptanceThreshold,
            sensitivities: sensitivities
        );
    }
}