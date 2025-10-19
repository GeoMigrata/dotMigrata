using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Core.Domain.Values;

namespace dotGeoMigrata.Common.Utilities;

internal static class FactorValidator
{
    public static void EnsureDefined(World world, FactorDefinition factor)
    {
        if (!world.FactorDefinitions.Contains(factor))
            throw new ArgumentException("Factor not defined", nameof(factor));
    }
}