namespace dotGeoMigrata.Core.Domain.Values;

public sealed record FactorValue
{
    public required FactorDefinition Factor { get; init; }
    public required double Intensity { get; set; }

    internal double Normalize(FactorDefinition fd) => fd.Normalize(Intensity);
}