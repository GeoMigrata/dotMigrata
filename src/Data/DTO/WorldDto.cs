using dotGeoMigrata.Interfaces;

namespace dotGeoMigrata.Data.DTO;

internal record WorldDto : IIdentifiable
{
    public string DisplayName { get; init; } = "New World";

    public required Dictionary<string, CityDto> Cities { get; init; }
    public required Dictionary<string, FactorDefinitionDto> FactorDefinitions { get; init; }
    
    public EngineConfigDto? EngineConfigOverridden { get; init; }
}