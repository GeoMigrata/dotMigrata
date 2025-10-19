using dotGeoMigrata.Core.Domain.Entities;

namespace dotGeoMigrata.Core.Domain.Values;

public record struct FactorValue
{
    public required FactorDefinition Factor { get; init; }
    public required double Intensity { get; set; }

    private double Normalize(FactorDefinition fd) => fd.Normalize(Intensity);
}