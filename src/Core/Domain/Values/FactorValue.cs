using dotGeoMigrata.Core.Domain.Entities;

namespace dotGeoMigrata.Core.Domain.Values;

internal record struct FactorValue
{
    public FactorValue(string factorId, double value)
    {
        // FactorId = !string.IsNullOrWhiteSpace(factorId)
        //     ? World.Factors.Any(f => f.Id == factorId)
        //         ? factorId
        //         : throw new ArgumentException($"{factorId} is not a valid factorId")
        //     : throw new ArgumentException("Invalid Factor Id", nameof(factorId));
        if (string.IsNullOrWhiteSpace(factorId))
            throw new ArgumentException("Invalid Factor Id", nameof(factorId));
        if (World.Factors.All(f => f.Id != factorId))
            throw new ArgumentException($"{factorId} does not exist", nameof(factorId));

        FactorId = factorId;
        Intensity = value;
    }

    public string FactorId { get; init; }
    public double Intensity { get; set; }
}