using dotGeoMigrata.Core.Domain.Entities;

namespace dotGeoMigrata.Core.Domain.Values;

internal record struct FactorValue
{
    public required FactorDefinition Factor { get; init; }
    public required double Intensity { get; set; }

    private double Normalize(FactorDefinition fd) => fd.Normalize(Intensity);
}