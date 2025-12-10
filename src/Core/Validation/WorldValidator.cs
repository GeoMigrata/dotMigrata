using dotMigrata.Core.Entities;
using dotMigrata.Core.Exceptions;

namespace dotMigrata.Core.Validation;

/// <summary>
/// Provides comprehensive validation for <see cref="World"/> instances before simulation execution.
/// </summary>
/// <remarks>
/// Validates world structure, factor consistency, and data integrity to prevent runtime errors.
/// </remarks>
public static class WorldValidator
{
    /// <summary>
    /// Validates a world instance for simulation readiness.
    /// </summary>
    /// <param name="world">The world to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="world"/> is null.</exception>
    /// <exception cref="WorldValidationException">Thrown when validation fails.</exception>
    public static void Validate(World world)
    {
        ArgumentNullException.ThrowIfNull(world);

        ValidateFactorConsistency(world);
        ValidateCityData(world);
        ValidatePersonData(world);
        ValidatePopulationDistribution(world);
    }

    /// <summary>
    /// Validates that all cities have factor values for all factor definitions.
    /// </summary>
    private static void ValidateFactorConsistency(World world)
    {
        foreach (var city in world.Cities)
        {
            var missingFactors = world.FactorDefinitions
                .Where(fd => city.FactorValues.All(fv => fv.Definition != fd))
                .ToList();

            if (missingFactors.Count > 0)
                throw new WorldValidationException(
                    city.DisplayName, 
                    missingFactors.Select(f => f.DisplayName));
        }
    }

    /// <summary>
    /// Validates city-specific data integrity.
    /// </summary>
    private static void ValidateCityData(World world)
    {
        if (world.Cities.Count == 0)
            throw new ConfigurationException("World must contain at least one city.");

        var cityNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var city in world.Cities)
        {
            if (string.IsNullOrWhiteSpace(city.DisplayName))
                throw new ConfigurationException("City display name cannot be null or empty.");

            if (!cityNames.Add(city.DisplayName))
                throw new ConfigurationException(
                    $"Duplicate city name detected: '{city.DisplayName}'. City names must be unique.");

            if (city.Area <= 0)
                throw new ConfigurationException(
                    $"City '{city.DisplayName}' has invalid area: {city.Area}. Area must be greater than zero.");

            if (city.Capacity is < 0)
                throw new ConfigurationException(
                    $"City '{city.DisplayName}' has invalid capacity: {city.Capacity}. Capacity cannot be negative.");
        }
    }

    /// <summary>
    /// Validates person data integrity.
    /// </summary>
    private static void ValidatePersonData(World world)
    {
        if (world.Population == 0)
            throw new ConfigurationException("World must contain at least one person.");

        foreach (var city in world.Cities)
        {
            foreach (var person in city.Persons)
            {
                if (person.CurrentCity != city)
                    throw new ConfigurationException(
                        $"Person in city '{city.DisplayName}' has mismatched CurrentCity reference.");

                // Validate that person has sensitivities for all factors
                foreach (var factor in world.FactorDefinitions)
                    _ = person.GetSensitivity(factor); // This validates the factor exists
            }
        }
    }

    /// <summary>
    /// Validates population distribution across cities.
    /// </summary>
    private static void ValidatePopulationDistribution(World world)
    {
        foreach (var city in world.Cities)
        {
            if (city.Capacity is not > 0) continue;
            if (city.Population > city.Capacity.Value * 2)
            {
                // Allow some overpopulation, but warn if severely over capacity
                Console.WriteLine(
                    $"Warning: City '{city.DisplayName}' population ({city.Population}) " +
                    $"significantly exceeds capacity ({city.Capacity.Value})");
            }
        }
    }

    /// <summary>
    /// Tries to validate a world instance without throwing exceptions.
    /// </summary>
    /// <param name="world">The world to validate.</param>
    /// <param name="errorMessage">The error message if validation fails.</param>
    /// <returns>True if validation succeeds; otherwise, false.</returns>
    public static bool TryValidate(World world, out string? errorMessage)
    {
        try
        {
            Validate(world);
            errorMessage = null;
            return true;
        }
        catch (Exception ex) when (ex is WorldValidationException or ConfigurationException)
        {
            errorMessage = ex.Message;
            return false;
        }
    }
}