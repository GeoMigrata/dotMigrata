using dotMigrata.Core.Values;

namespace dotMigrata.Logic.Common;

/// <summary>
/// Provides common mathematical functions for migration calculations.
/// </summary>
/// <remarks>
/// All methods returning values in the [0, 1] range now return <see cref="UnitValue"/> for type safety.
/// </remarks>
public static class MathUtils
{
    /// <summary>
    /// Calculates the sigmoid function for smooth probability mapping.
    /// </summary>
    /// <param name="x">Input value.</param>
    /// <param name="steepness">Controls the steepness of the curve (default: 10).</param>
    /// <param name="midpoint">The midpoint where sigmoid equals 0.5 (default: 0).</param>
    /// <returns>A value between 0 and 1 as <see cref="UnitValue"/>.</returns>
    public static UnitValue Sigmoid(double x, double steepness, double midpoint = .0)
    {
        var result = 1.0 / (1.0 + Math.Exp(-steepness * (x - midpoint)));
        return UnitValue.FromRatio(result);
    }

    /// <summary>
    /// Calculates exponential distance decay.
    /// </summary>
    /// <param name="distance">The distance in kilometers.</param>
    /// <param name="lambda">The decay coefficient (default: 0.001).</param>
    /// <returns>A decay factor between 0 and 1 as <see cref="UnitValue"/>.</returns>
    public static UnitValue DistanceDecay(double distance, double lambda = .001)
    {
        var result = Math.Exp(-lambda * distance);
        return UnitValue.FromRatio(result);
    }


    /// <summary>
    /// Calculates capacity resistance based on current population and capacity.
    /// </summary>
    /// <param name="currentPopulation">Current population in the city.</param>
    /// <param name="capacity">Maximum capacity of the city.</param>
    /// <param name="steepness">Controls how sharply resistance increases near capacity (default: 5).</param>
    /// <returns>A resistance factor between 0 and 1 (higher = more resistance) as <see cref="UnitValue"/>.</returns>
    public static UnitValue CapacityResistance(double currentPopulation, double capacity, double steepness = 5.0)
    {
        if (capacity <= 0)
            return UnitValue.Zero;

        var crowdRatio = currentPopulation / capacity;
        return Sigmoid(crowdRatio, steepness, 1.0);
    }

    /// <summary>
    /// Normalizes a collection of values to sum to 1 (softmax for probabilities).
    /// </summary>
    /// <param name="values">The values to normalize.</param>
    /// <returns>Normalized values that sum to 1 as <see cref="UnitValue"/> collection.</returns>
    public static IReadOnlyList<UnitValue> Softmax(IEnumerable<double> values)
    {
        var valueList = values.ToList();
        if (valueList.Count == 0)
            return [];

        var expValues = valueList.Select(Math.Exp).ToList();
        var sumExp = expValues.Sum();

        if (sumExp != 0)
            return expValues.Select(exp => UnitValue.FromRatio(exp / sumExp)).ToList();

        var uniformProb = 1.0 / valueList.Count;
        return valueList.Select(_ => UnitValue.FromRatio(uniformProb)).ToList();
    }

    /// <summary>
    /// Linearly normalizes a value to the range [0, 1].
    /// </summary>
    /// <param name="value">The value to normalize.</param>
    /// <param name="min">Minimum value in the range.</param>
    /// <param name="max">Maximum value in the range.</param>
    /// <returns>Normalized value between 0 and 1 as <see cref="UnitValue"/>.</returns>
    public static UnitValue LinearNormalize(double value, double min, double max)
    {
        if (max <= min)
            return UnitValue.Zero;

        var clamped = Math.Clamp(value, min, max);
        var result = (clamped - min) / (max - min);
        return UnitValue.FromRatio(result);
    }
}