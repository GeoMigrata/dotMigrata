using dotGeoMigrata.Core.Domain.Enums;

namespace dotGeoMigrata.Core.Domain.Values;

public readonly record struct FactorSensitivity
{
    public required FactorDefinition Factor { get; init; }
    public required int Sensitivity { get; init; }
    public required FactorType? OverriddenFactorType { get; init; }
}