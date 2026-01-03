using dotMigrata.Core.Entities;
using dotMigrata.Core.Enums;
using dotMigrata.Core.Values;
using dotMigrata.Logic.Common;
using dotMigrata.Logic.Interfaces;
using dotMigrata.Logic.Models;

namespace dotMigrata.Logic.Calculators;

/// <summary>
/// Standard attraction calculator implementing optimized individual-based calculations.
/// Calculates city attraction based on person's factor sensitivities, capacity resistance, and distance decay.
/// </summary>
public sealed class StandardAttractionCalculator : IAttractionCalculator
{
    private readonly StandardModelConfig _config;

    /// <summary>
    /// Initializes a new instance of the StandardAttractionCalculator.
    /// </summary>
    /// <param name="config">Configuration parameters for the calculator. If null, uses default configuration.</param>
    /// <exception cref="Core.Exceptions.ConfigurationException">Thrown when the configuration is invalid.</exception>
    public StandardAttractionCalculator(StandardModelConfig? config = null) =>
        _config = (config ?? StandardModelConfig.Default).Validate();

    /// <inheritdoc />
    public AttractionResult CalculateAttraction(City city, PersonBase person, City? originCity = null)
    {
        // Step 1: Calculate base attraction from factor system
        var baseAttraction = CalculateBaseAttraction(city, person);

        // Step 2: Calculate capacity resistance
        var capacityResistance = CalculateCapacityResistance(city);

        // Step 3: Calculate distance resistance (if origin city provided)
        var distanceResistance = UnitValue.One;
        if (originCity != null)
            distanceResistance = CalculateDistanceResistance(originCity, city);

        return new AttractionResult
        {
            City = city,
            BaseAttraction = baseAttraction,
            CapacityResistance = capacityResistance,
            DistanceResistance = distanceResistance
        };
    }

    /// <inheritdoc />
    public IDictionary<City, AttractionResult> CalculateAttractionForAllCities(
        IEnumerable<City> cities,
        PersonBase person,
        City? originCity = null) =>
        cities.ToDictionary(
            city => city,
            city => CalculateAttraction(city, person, originCity));

    /// <summary>
    /// Calculates the base attraction score from the factor system for an individual person.
    /// Formula: A_p = Σ(S_pk * I_ck_adj) where:
    /// - S_pk: sensitivity of person p to factor k
    /// - I_ck_adj: adjusted intensity of factor k in city c
    ///     - For Positive factors: I_ck_adj = I_ck
    ///     - For Negative factors: I_ck_adj = (1 - I_ck)
    /// This ensures lower intensity of negative factors increases attraction.
    /// </summary>
    private static UnitValue CalculateBaseAttraction(City city, PersonBase person)
    {
        var totalScore = 0.0;

        foreach (var (factor, sensitivity) in person.FactorSensitivities)
        {
            if (!city.TryGetFactorIntensity(factor, out var factorIntensity))
                continue;

            // Intensity values are already normalized UnitValues (0-1 range)
            var normalizedIntensity = factorIntensity.Value;

            // Adjust intensity based on factor direction
            var adjustedIntensity = factor.Type == FactorType.Positive
                ? normalizedIntensity
                : (UnitValue.One - normalizedIntensity);

            // Calculate contribution: sensitivity * adjusted_intensity
            totalScore += (double)sensitivity * (double)adjustedIntensity;
        }

        // Apply person's sensitivity scaling if it's a StandardPerson
        if (person is StandardPerson stdPerson)
            totalScore *= stdPerson.SensitivityScaling;

        // Normalize to [0, 1] range using sigmoid
        return MathUtils.Sigmoid(totalScore, 1.0);
    }

    /// <summary>
    /// Calculates capacity resistance based on current population vs capacity.
    /// Formula: R_c = 1 / (1 + e^(-k_c * (crowd - 1)))
    /// where crowd = currentPop / capacity
    /// </summary>
    private UnitValue CalculateCapacityResistance(City city)
    {
        if (city.Capacity is null or 0)
            return UnitValue.Zero; // No capacity limit means no resistance

        return MathUtils.CapacityResistance(
            city.Population,
            city.Capacity.Value,
            _config.CapacitySteepness);
    }

    /// <summary>
    /// Calculates distance resistance using exponential decay.
    /// Formula: R_d = e^(-λ * distance)
    /// </summary>
    private UnitValue CalculateDistanceResistance(City originCity, City destinationCity)
    {
        var distance = originCity.Location.DistanceTo(destinationCity.Location);
        return MathUtils.DistanceDecay(distance, _config.DistanceDecayLambda);
    }
}