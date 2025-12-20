using dotMigrata.Core.Entities;
using dotMigrata.Core.Interfaces;
using dotMigrata.Logic.Common;

namespace dotMigrata.Core.Strategies;

/// <summary>
/// Default soft capacity strategy with sigmoid-based resistance.
/// </summary>
/// <remarks>
///     <para>
///     This strategy doesn't enforce hard limits but increases resistance as cities approach capacity.
///     Uses a sigmoid function to create smooth, realistic capacity pressure curves.
///     </para>
///     <para>
///     This is the default strategy and maintains backward compatibility with existing simulations.
///     </para>
/// </remarks>
public sealed class SoftCapacityStrategy : ICityCapacityStrategy
{
    private readonly double _resistanceSteepness;

    /// <summary>
    /// Initializes a new instance with custom resistance steepness.
    /// </summary>
    /// <param name="resistanceSteepness">Controls how sharply resistance increases near capacity. Default is 5.0.</param>
    public SoftCapacityStrategy(double resistanceSteepness = 5.0)
    {
        _resistanceSteepness = resistanceSteepness;
    }

    /// <summary>
    /// Gets the default instance with standard resistance curve.
    /// </summary>
    public static SoftCapacityStrategy Default { get; } = new();

    /// <inheritdoc />
    public string StrategyName => "SoftCap";

    /// <inheritdoc />
    public bool CanAccommodate(City city, int additionalPopulation)
    {
        // Soft cap always allows migration, but with increasing resistance
        return true;
    }

    /// <inheritdoc />
    public double CalculateCapacityResistance(City city)
    {
        return city.Capacity is null or <= 0
            ? 0.0
            : MathUtils.CapacityResistance(city.Population, city.Capacity.Value, _resistanceSteepness);
    }

    /// <inheritdoc />
    public double GetCapacityUtilization(City city)
    {
        if (city.Capacity is null or <= 0)
            return 0.0;

        return (double)city.Population / city.Capacity.Value;
    }
}