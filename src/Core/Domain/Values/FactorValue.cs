using dotGeoMigrata.Core.Domain.Entities;

namespace dotGeoMigrata.Core.Domain.Values;

internal record struct FactorValue(FactorDefinition Factor, double Intensity)
{
    public FactorDefinition Factor { get; init; } = Factor;

    private double Normalize(FactorDefinition fd) => fd.Normalize(Intensity);
}