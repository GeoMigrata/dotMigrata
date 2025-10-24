using dotGeoMigrata.Core.Domain.Entities;
using dotGeoMigrata.Core.Domain.Enums;
using dotGeoMigrata.Core.Domain.Values;

namespace dotGeoMigrata.Logic.Attraction;

/// <summary>
/// Calculates city attraction scores for population groups based on factors and sensitivities.
/// </summary>
public sealed class AttractionCalculator
{
    /// <summary>
    /// Calculates the attraction score of a city for a specific population group.
    /// </summary>
    /// <param name="city">The city to evaluate.</param>
    /// <param name="group">The population group.</param>
    /// <param name="world">The world context for factor definitions.</param>
    /// <returns>The attraction result.</returns>
    public AttractionResult CalculateAttraction(City city, PopulationGroup group, World world)
    {
        if (city == null) throw new ArgumentNullException(nameof(city));
        if (group == null) throw new ArgumentNullException(nameof(group));
        if (world == null) throw new ArgumentNullException(nameof(world));

        double totalScore = 0.0;

        // Calculate weighted sum of normalized factors
        foreach (var sensitivity in group.Sensitivities)
        {
            var factorValue = city.FactorValues.FirstOrDefault(fv => fv.Factor == sensitivity.Factor);
            if (factorValue == null) continue;

            // Normalize the factor value
            var normalizedValue = factorValue.Normalize(sensitivity.Factor);

            // Determine factor type (use override if specified)
            var factorType = sensitivity.OverriddenFactorType ?? sensitivity.Factor.Type;

            // Apply factor direction: Positive factors add to attraction, Negative factors subtract
            var contribution = factorType == FactorType.Positive
                ? normalizedValue * sensitivity.Sensitivity
                : -normalizedValue * sensitivity.Sensitivity;

            totalScore += contribution;
        }

        return new AttractionResult
        {
            City = city,
            PopulationGroup = group,
            AttractionScore = totalScore
        };
    }

    /// <summary>
    /// Calculates attraction scores for a population group across all cities.
    /// </summary>
    /// <param name="world">The world containing cities.</param>
    /// <param name="group">The population group.</param>
    /// <returns>Collection of attraction results for all cities.</returns>
    public IReadOnlyList<AttractionResult> CalculateAttractionForAllCities(World world, PopulationGroup group)
    {
        if (world == null) throw new ArgumentNullException(nameof(world));
        if (group == null) throw new ArgumentNullException(nameof(group));

        var results = new List<AttractionResult>();

        foreach (var city in world.Cities)
        {
            var result = CalculateAttraction(city, group, world);
            results.Add(result);
        }

        return results;
    }
}