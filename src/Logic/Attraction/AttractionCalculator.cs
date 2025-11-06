using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Enums;
using dotGeoMigrata.Core.Values;
using dotGeoMigrata.Logic.Interfaces;

namespace dotGeoMigrata.Logic.Attraction;

/// <summary>
/// Calculates city attraction scores for population groups based on factors and sensitivities.
/// </summary>
/// This is the original implementation maintained for backward compatibility.
/// <remarks>
/// This implementation is deprecated. Use <see cref="EnhancedAttractionCalculator" /> for new projects.
/// The enhanced version implements the pull-push factor model per LogicModel.md specification.
/// </remarks>
[Obsolete("Use EnhancedAttractionCalculator for new projects. " +
          "This version is maintained for backward compatibility only.")]
public sealed class AttractionCalculator : IAttractionCalculator
{
    /// <summary>
    /// Calculates the attraction score of a city for a specific population group definition.
    /// </summary>
    /// <param name="city">The city to evaluate.</param>
    /// <param name="groupDefinition">The population group definition.</param>
    /// <param name="world">The world context for factor definitions.</param>
    /// <returns>The attraction result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public AttractionResult CalculateAttraction(City city, PopulationGroupDefinition groupDefinition,
        World world)
    {
        ArgumentNullException.ThrowIfNull(city, nameof(city));
        ArgumentNullException.ThrowIfNull(groupDefinition, nameof(groupDefinition));
        ArgumentNullException.ThrowIfNull(world, nameof(world));

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
        ArgumentNullException.ThrowIfNull(world, nameof(world));
        ArgumentNullException.ThrowIfNull(groupDefinition, nameof(groupDefinition));

        return world.Cities
            .Select(city => CalculateAttraction(city, groupDefinition, world))
            .ToList();
    }
}