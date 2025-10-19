using dotGeoMigrata.Core.Domain.Values;
using dotGeoMigrata.Interfaces;

namespace dotGeoMigrata.Data.DTO;

internal record CityDto : IIdentifiable
{
    public required string DisplayName { get; init; }
    
    public required double Area { get; init; }
    public required Coordinate Position { get; init; }
    public required Dictionary<string, double> FactorValues { get; init; }
    public required Dictionary<string, PopulationGroupDto> PopulationGroups { get; init; }
}