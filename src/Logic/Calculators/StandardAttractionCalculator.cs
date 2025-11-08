using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Enums;
using dotGeoMigrata.Core.Values;
using dotGeoMigrata.Logic.Common;
using dotGeoMigrata.Logic.Interfaces;
using dotGeoMigrata.Logic.Models;

namespace dotGeoMigrata.Logic.Calculators;

/// <summary>
/// Standard attraction calculator implementing the model described in model.md.
/// Calculates city attraction based on factor sensitivities, capacity resistance, and distance decay.
/// </summary>
public sealed class StandardAttractionCalculator : IAttractionCalculator
{
    private readonly StandardModelConfig _config;

    /// <summary>
    /// Initializes a new instance of the StandardAttractionCalculator.
    /// </summary>
    /// <param name="config">Configuration parameters for the calculator. If null, uses default configuration.</param>
    public StandardAttractionCalculator(StandardModelConfig? config = null)
    {
        _config = config ?? StandardModelConfig.Default;
    }

    /// <inheritdoc />
    public AttractionResult CalculateAttraction(City city, GroupDefinition group, City? originCity = null)
    {
        // Step 1: Calculate base attraction from factor system
        var baseAttraction = CalculateBaseAttraction(city, group);

        // Step 2: Calculate capacity resistance
        var capacityResistance = CalculateCapacityResistance(city);

        // Step 3: Calculate distance resistance (if origin city provided
        var distanceResistance = 1.0;
        if (originCity != null)
            distanceResistance = CalculateDistanceResistance(originCity, city);

        // Step 4: Calculate adjusted attraction
        var adjustedAttraction = baseAttraction * (1.0 - capacityResistance) * distanceResistance;

        return new AttractionResult
        {
            City = city,
            BaseAttraction = baseAttraction,
            AdjustedAttraction = adjustedAttraction,
            CapacityResistance = capacityResistance,
            DistanceResistance = distanceResistance
        };
    }

    public IDictionary<City, AttractionResult> CalculateAttractionForAllCities(IEnumerable<City> cities,
        GroupDefinition group, City? originCity = null)
    {
        return cities.ToDictionary(
            city => city,
            city => CalculateAttraction(city, group, originCity));
    }

    /// <summary>
    /// Calculates the base attraction score from the factor system.
    /// Formula: A_ij = Σ(S_ik * I_jk * D_k) where:
    /// - S_ik: sensitivity of group i to factor k
    /// - I_jk: normalized intensity of factor k in city j
    /// - D_k: direction of factor k (+1 for positive, -1 for negative)
    /// </summary>
    private static double CalculateBaseAttraction(City city, GroupDefinition group)
    {
        var totalScore = .0;

        foreach (var sensitivity in group.Sensitivities)
        {
            if (!city.TryGetFactorValue(sensitivity.Factor, out var factorValue))
                continue;

            // Normalize the factor intensity
            var normalizedIntensity = factorValue!.Normalize(sensitivity.Factor);

            // Determine the factor direction (use override if specified)
            var factorType = sensitivity.OverriddenFactorType ?? sensitivity.Factor.Type;
            var direction = factorType == FactorType.Positive ? 1.0 : -1.0;

            // Calculate contribution: sensitivity * normalized_intensity * direction
            totalScore += sensitivity.Sensitivity * normalizedIntensity * direction;
        }

        // Apply sensitivity scaling if specified
        totalScore *= group.SensitivityScaling;

        // Normalize to [0, 1] range - we'll use a sigmoid to keep it bounded
        // but allow both positive and negative total scores to be meaningful
        return MathUtils.Sigmoid(totalScore, 1.0);
    }

    /// <summary>
    /// Calculates capacity resistance based on current population vs capacity.
    /// Formula: R_c = 1 / (1 + e^(-k_c * (crowd - 1)))
    /// where crowd = currentPop / capacity
    /// </summary>
    private double CalculateCapacityResistance(City city)
    {
        if (city.Capacity is null or 0)
            return .0; // No capacity limit means no resistance

        return MathUtils.CapacityResistance(
            city.Population,
            city.Capacity.Value,
            _config.CapacitySteepness);
    }

    /// <summary>
    /// Calculates distance resistance using exponential decay.
    /// Formula: R_d = e^(-λ * distance)
    /// </summary>
    private double CalculateDistanceResistance(City originCity, City destinationCity)
    {
        var distance = originCity.Location.DistanceTo(destinationCity.Location);
        return MathUtils.DistanceDecay(distance, _config.DistanceDecayLambda);
    }
}