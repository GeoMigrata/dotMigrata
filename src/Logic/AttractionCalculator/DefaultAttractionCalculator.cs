using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Core.Domain.Values;
using dotGeoMigrata.Interfaces;
using dotGeoMigrata.Interfaces.Logic;

namespace dotGeoMigrata.Logic.AttractionCalculator;

/// <summary>
/// Default implementation of IAttractionCalculator.
/// Computes attraction based on group sensitivities, moving willingness, city factor values, and distance.
/// </summary>
public class DefaultAttractionCalculator : IAttractionCalculator
{
    private readonly double _distanceDecayFactor;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="distanceDecayFactor">
    /// Optional distance decay factor. 0 means distance is ignored.
    /// Higher values reduce attraction with distance.
    /// </param>
    public DefaultAttractionCalculator(double distanceDecayFactor = 0.0) =>
        _distanceDecayFactor = distanceDecayFactor;

    public double ComputeAttraction(PopulationGroup group, City targetCity, City currentCity)
    {
        if (group.Sensitivities.Count == 0) return .0;

        // Sum of weighted factor intensities
        var factorSum = (from fs in @group.Sensitivities
            let fv = targetCity.FactorValues.FirstOrDefault(fv => fv.Factor == fs.Factor)
            let intensity = fv.Intensity
            select fs.Sensitivity * intensity).Sum();

        // Multiply by group's moving willingness
        var attraction = group.MovingWillingness * factorSum;

        // Apply distance decay if currentCity is provided
        if (!(_distanceDecayFactor > 0)) return attraction;
        var distance = Coordinate.CalculateDistance(currentCity.Position, targetCity.Position);
        attraction *= Math.Exp(-_distanceDecayFactor * distance);

        return attraction;
    }
}