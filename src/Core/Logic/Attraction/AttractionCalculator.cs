using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Core.Domain.Values;

namespace dotGeoMigrata.Core.Logic.Attraction;

/// <summary>
/// Calculates the attraction score of a city for a specific population group.
/// </summary>
internal static class AttractionCalculator
{
    /// <summary>
    /// Compute the attraction score for a given (City, PopulationGroup)
    /// </summary>
    // public static double ComputeAttraction(City c, PopulationGroup g)
    // {
    //     var factors = World.Factors;
    //     var factorValues = c.FactorValues;
    //
    //     var totalWeight = .0;
    //     var weightedSum = .0;
    //
    //     var factorIdSet = factors.Select(o => o.Id).ToHashSet();
    //
    //     foreach (var fs in g.Sensitivities)
    //     {
    //         if (!factorIdSet.Contains(fs.FactorId)) continue;
    //         
    //         var fd = factors.FirstOrDefault(f => f.Id == fs.FactorId);
    //         var type = fs.OverriddenFactorType ?? fd.Type;
    //         
    //         // Apply direction: positive means attraction, negative means repulsion
    //         var signed = direction == FactorDirection.Positive ? normalized : 1.0 - normalized;
    //     }
    // }
}