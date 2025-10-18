using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Core.Domain.Enums;

namespace dotGeoMigrata.Simulation.Logic.Attraction;

/// <summary>
/// Calculates the attraction score of a city for a specific population group.
/// </summary>
internal static class AttractionCalculator
{
    /// <summary>
    /// Compute the attraction score for a given (City, PopulationGroup)
    /// </summary>
    public static double ComputeAttraction(World w, City c, PopulationGroup g)
    {
        var factorValues = c.FactorValues;

        var totalWeight = .0;
        var weightedSum = .0;

        var factorMap = w.Factors.ToDictionary(f => f.Id);

        foreach (var sens in g.Sensitivities)
        {
            if (!factorMap.TryGetValue(sens.FactorId, out var fd))
                continue;
            if (c.FactorValues.All(fv => fv.FactorId != sens.FactorId))
                continue;
            
            var intensity = c.FactorValues.FirstOrDefault(fv => fv.FactorId == sens.FactorId).Intensity;
            // var normalized = 
            var type = sens.OverriddenFactorType ?? fd.Type;

            // Apply direction: positive means attraction, negative means repulsion
            
            var signed = type == FactorType.Positive
                ? intensity
                : 1.0 - intensity;
            
            //foreach (var sens in group.Sensitivities.Values)
            // {
            //     if (!factorMap.TryGetValue(sens.FactorId, out var def))
            //         continue;
            //
            //     var val = city.GetFactorValue(sens.FactorId);
            //     if (val == null) continue;
            //
            //     var normalized = val.GetNormalized(def);
            //     var direction = sens.OverrideDirection ?? def.Direction;
            //
            //     // Apply direction: positive means attraction, negative means repulsion
            //     var signed = direction == FactorDirection.Positive ? normalized : 1.0 - normalized;
            //
            //     weightedSum += signed * sens.Weight;
            //     totalWeight += sens.Weight;
            // }

            weightedSum += signed * sens.Value;
            totalWeight += sens.Value;
        }

        return totalWeight > 0 ? weightedSum / totalWeight : .0;
    }
}