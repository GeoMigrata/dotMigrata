using dotGeoMigrata.Interfaces;

namespace dotGeoMigrata.Data.DTO;

internal record PopulationGroupDto : IIdentifiable
{
    public required string DisplayName { get; init; }

    public required int Count { get; init; }
    public required double MovingWillingness { get; init; }
    public required double MigrationThreshold { get; init; }
    public required double AcceptanceThreshold { get; init; }

    public required Dictionary<string, FactorSensitivityDto> FactorSensitivities { get; init; }
}