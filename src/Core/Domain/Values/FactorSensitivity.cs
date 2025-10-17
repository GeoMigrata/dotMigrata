using System.Runtime.InteropServices.ComTypes;
using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Core.Domain.Enums;

namespace dotGeoMigrata.Core.Domain.Values;

internal readonly record struct FactorSensitivity
{
    public required string FactorId { get; init; }
    public required int Value { get; init; }
    public required FactorType? OverriddenFactorType { get; init; }

    public FactorSensitivity(string factorId, int value, FactorType? overriddenFactorType = null)
    {
        if (string.IsNullOrWhiteSpace(factorId))
            throw new ArgumentException("Invalid Factor Id", nameof(factorId));
        if (World.Factors.All(f => f.Id != factorId))
            throw new ArgumentException($"{factorId} does not exist", nameof(factorId));
        FactorId = factorId;

        // if (World.Factors.FirstOrDefault(f => f.Id == factorId).Type is FactorType.Neutral &&
        //     overriddenFactorType is null or FactorType.Neutral)
        //     
        // switch (World.Factors.FirstOrDefault(f => f.Id == factorId).Type){
        //     case FactorType.Neutral: switch(overriddenFactorType)
        //     {
        //         case null:
        //             throw new ArgumentException(
        //                 "FactorType in FactorSensitivity must not be null when in FactorDefinition is Neutral",
        //                 nameof(overriddenFactorType));
        //             break;
        //         case FactorType.Neutral:
        //             throw new ArgumentException(
        //             "FactorType in FactorSensitivity and FactorDefinition must not be Neutral at same time",
        //             nameof(overriddenFactorType));
        //     }),
        //     _ => null
        // };
        Value = value;
    }
}