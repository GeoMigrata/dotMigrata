using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Enums;
using dotGeoMigrata.Core.Values;

namespace dotGeoMigrata.Logic.Attraction;

/// <summary>
/// Calculates city attraction scores for population groups based on factors and sensitivities.
/// </summary>
public sealed class AttractionCalculator
{
    /// <summary>
    /// Calculates the attraction score of a city for a specific population group definition.
    /// </summary>
    /// <param name="city">The city to evaluate.</param>
    /// <param name="groupDefinition">The population group definition.</param>
    /// <param name="world">The world context for factor definitions.</param>
    /// <returns>The attraction result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public static AttractionResult CalculateAttraction(City city, PopulationGroupDefinition groupDefinition, World world)
    {
        ArgumentNullException.ThrowIfNull(city);
        ArgumentNullException.ThrowIfNull(groupDefinition);
        ArgumentNullException.ThrowIfNull(world);

        var totalScore = 0.0;

        // Calculate weighted sum of normalized factors
        foreach (var sensitivity in groupDefinition.Sensitivities)
        {
            if (!city.TryGetFactorValue(sensitivity.Factor, out var factorValue) || factorValue is null)
                continue;

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
            PopulationGroupDefinition = groupDefinition,
            AttractionScore = totalScore
        };
    }

    /// <summary>
    /// Calculates attraction scores for a population group definition across all cities.
    /// </summary>
    /// <param name="world">The world containing cities.</param>
    /// <param name="groupDefinition">The population group definition.</param>
    /// <returns>Collection of attraction results for all cities.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public IReadOnlyList<AttractionResult> CalculateAttractionForAllCities(
        World world,
        PopulationGroupDefinition groupDefinition)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(groupDefinition);

        return world.Cities
            .Select(city => CalculateAttraction(city, groupDefinition, world))
            .ToList();
    }
}