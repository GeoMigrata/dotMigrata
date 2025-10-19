using System.Runtime.InteropServices.ComTypes;
using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Core.Domain.Enums;

namespace dotGeoMigrata.Core.Domain.Values;

internal readonly record struct FactorSensitivity
{
    public FactorDefinition Factor { get; init; }
    public int Sensitivity { get; init; }
    public FactorType? OverriddenFactorType { get; init; }

    public FactorSensitivity(World world, FactorDefinition factor, int sensitivity,
        FactorType? overriddenFactorType = null)
    {
        Factor = factor;
        Sensitivity = sensitivity;
    }
}