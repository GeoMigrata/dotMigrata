using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Enums;
using dotGeoMigrata.Core.Values;
using dotGeoMigrata.Logic.Interfaces;

namespace dotGeoMigrata.Logic.Attraction;

/// <summary>
/// Enhanced attraction calculator implementing the pull-push factor model.
/// This implementation follows the algorithm specified in LogicModel.md.
/// </summary>
/// <remarks>
/// The enhanced calculator separates pull and push factors, applies group sensitivity scaling,
/// and supports configurable normalization methods for more accurate attraction modeling.
/// </remarks>
public sealed class EnhancedAttractionCalculator : IAttractionCalculator
{
    /// <summary>
    /// Calculates the attraction score of a city for a specific population group using pull-push model.
    /// </summary>
    /// <param name="city">The city to evaluate.</param>
    /// <param name="groupDefinition">The population group definition.</param>
    /// <param name="world">The world context for factor definitions.</param>
    /// <returns>The attraction result with separated pull and push components.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public AttractionResult CalculateAttraction(City city, PopulationGroupDefinition groupDefinition, World world)
    {
        ArgumentNullException.ThrowIfNull(city, nameof(city));
        ArgumentNullException.ThrowIfNull(groupDefinition, nameof(groupDefinition));
        ArgumentNullException.ThrowIfNull(world, nameof(world));

        double pullSum = 0.0;
        double pushSum = 0.0;

        // Calculate weighted sum of normalized factors, separated by pull/push direction
        foreach (var sensitivity in groupDefinition.Sensitivities)
        {
            if (!city.TryGetFactorValue(sensitivity.Factor, out var factorValue) || factorValue is null)
                continue;

            // Normalize the factor value to [0,1]
            var normalizedValue = factorValue.Normalize(sensitivity.Factor);

            // Determine factor type (use override if specified, otherwise use factor definition's type)
            var factorType = sensitivity.OverriddenFactorType ?? sensitivity.Factor.Type;

            // Calculate weighted contribution
            var weightedContribution = normalizedValue * sensitivity.Sensitivity;

            // Separate pull and push factors according to LogicModel.md
            if (factorType == FactorType.Positive)
            {
                // Pull factor: higher normalized value increases attraction
                pullSum += weightedContribution;
            }
            else
            {
                // Push factor: higher normalized value decreases attraction (repels)
                // Push contribution uses (1 - normalized_value) so that higher push values reduce attraction
                pushSum += (1.0 - normalizedValue) * sensitivity.Sensitivity;
            }
        }

        // Calculate comprehensive attraction index: Attraction(C,G) = A_G * (Pull - Push)
        // A_G is the group sensitivity scaling coefficient (default 1.0)
        var sensitivityScaling = groupDefinition.SensitivityScaling;
        var attractionScore = sensitivityScaling * (pullSum - pushSum);

        return new AttractionResult
        {
            City = city,
            PopulationGroupDefinition = groupDefinition,
            AttractionScore = attractionScore,
            PullComponent = pullSum,
            PushComponent = pushSum
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