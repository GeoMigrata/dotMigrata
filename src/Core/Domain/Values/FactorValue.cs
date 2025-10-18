using dotGeoMigrata.Core.Domain.Entities;

namespace dotGeoMigrata.Core.Domain.Values;

internal record struct FactorValue
{
    public FactorValue(World world, string factorId, double value)
    {
        // FactorId = !string.IsNullOrWhiteSpace(factorId)
        //     ? World.Factors.Any(f => f.Id == factorId)
        //         ? factorId
        //         : throw new ArgumentException($"{factorId} is not a valid factorId")
        //     : throw new ArgumentException("Invalid Factor Id", nameof(factorId));
        if (string.IsNullOrWhiteSpace(factorId))
            throw new ArgumentException("Invalid Factor Id", nameof(factorId));
        if (world.Factors.All(f => f.Id != factorId))
            throw new ArgumentException($"{factorId} does not exist", nameof(factorId));

        FactorId = factorId;
        Intensity = value;
    }

    public string FactorId { get; init; }
    public double Intensity { get; set; }

    private double Normalize(FactorDefinition fd) => fd.Id == FactorId
        ? fd.Normalize(Intensity)
        : throw new ArgumentException($"Definition Id {fd.Id} does not match with {FactorId}", nameof(fd));
}