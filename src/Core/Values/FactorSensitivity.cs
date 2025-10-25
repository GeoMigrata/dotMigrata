using dotGeoMigrata.Core.Enums;

namespace dotGeoMigrata.Core.Values;

public readonly record struct FactorSensitivity
{
    public required FactorDefinition Factor { get; init; }
    public required int Sensitivity { get; init; }
    public FactorType? OverriddenFactorType { get; init; }
}