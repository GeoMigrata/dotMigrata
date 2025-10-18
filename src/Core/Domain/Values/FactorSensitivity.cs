using System.Runtime.InteropServices.ComTypes;
using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Core.Domain.Enums;

namespace dotGeoMigrata.Core.Domain.Values;

internal readonly record struct FactorSensitivity
{
    public string FactorId { get; init; }
    public int Value { get; init; }
    public FactorType? OverriddenFactorType { get; init; }

    public FactorSensitivity(World world, string factorId, int value, FactorType? overriddenFactorType = null)
    {
        if (string.IsNullOrWhiteSpace(factorId))
            throw new ArgumentException("Invalid Factor Id", nameof(factorId));
        if (world.Factors.All(f => f.Id != factorId))
            throw new ArgumentException($"{factorId} does not exist", nameof(factorId));
        FactorId = factorId;
        Value = value;
    }
}