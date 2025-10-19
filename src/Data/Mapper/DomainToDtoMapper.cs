using dotGeoMigrata.Common.Utilities;
using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Data.DTO;

namespace dotGeoMigrata.Data.Mapper;

internal static class DomainToDtoMapper
{
    private static CityDto CreateCityDto(City city)
    {
        var factorValues = city.FactorValues.ToDictionary(
            f => JsonKeyHelper.ToSafeKey(f.Factor.DisplayName),
            f => f.Intensity);

        var populationGroups = city.PopulationGroups.ToDictionary(
            g => JsonKeyHelper.ToSafeKey(g.DisplayName),
            CreatePopulationGroupDto);

        return new CityDto
        {
            DisplayName = city.DisplayName,
            Area = city.Area,
            PopulationGroups = populationGroups,
            FactorValues = factorValues,
            Position = city.Position,
        };
    }

    private static PopulationGroupDto CreatePopulationGroupDto(PopulationGroup populationGroup)
    {
        var factorSensitivities = populationGroup.Sensitivities.ToDictionary(
            s => JsonKeyHelper.ToSafeKey(s.Factor.DisplayName),
            s => new FactorSensitivityDto
            {
                Value = s.Sensitivity,
                OverriddenFactorType = s.OverriddenFactorType
            });

        return new PopulationGroupDto
        {
            DisplayName = populationGroup.DisplayName,
            Count = populationGroup.Count,
            MinAcceptanceThreshold = populationGroup.MinAcceptanceThreshold,
            MaxMigrationThreshold = populationGroup.MaxMigrationThreshold,
            MovingWillingness = populationGroup.MovingWillingness,
            FactorSensitivities = factorSensitivities
        };
    }
}