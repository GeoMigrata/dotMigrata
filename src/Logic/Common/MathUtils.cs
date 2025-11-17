namespace dotMigrata.Logic.Common;

/// <summary>
/// Provides common mathematical functions for migration calculations.
/// </summary>
public static class MathUtils
{
    /// <summary>
    /// Calculates the sigmoid function for smooth probability mapping.
    /// </summary>
    /// <param name="x">Input value.</param>
    /// <param name="steepness">Controls the steepness of the curve (default: 10).</param>
    /// <param name="midpoint">The midpoint where sigmoid equals 0.5 (default: 0).</param>
    /// <returns>A value between 0 and 1.</returns>
    public static double Sigmoid(double x, double steepness, double midpoint = .0)
    {
        return 1.0 / (1.0 + Math.Exp(-steepness * (x - midpoint)));
    }


    /// <summary>
    /// Calculates exponential distance decay.
    /// </summary>
    /// <param name="distance">The distance in kilometers.</param>
    /// <param name="lambda">The decay coefficient (default: 0.001).</param>
    /// <returns>A decay factor between 0 and 1.</returns>
    public static double DistanceDecay(double distance, double lambda = .001)
    {
        return Math.Exp(-lambda * distance);
    }


    /// <summary>
    /// Calculates capacity resistance based on current population and capacity.
    /// </summary>
    /// <param name="currentPopulation">Current population in the city.</param>
    /// <param name="capacity">Maximum capacity of the city.</param>
    /// <param name="steepness">Controls how sharply resistance increases near capacity (default: 5).</param>
    /// <returns>A resistance factor between 0 and 1 (higher = more resistance).</returns>
    public static double CapacityResistance(double currentPopulation, double capacity, double steepness = 5.0)
    {
        if (capacity <= 0)
            return .0;

        var crowdRatio = currentPopulation / capacity;
        return Sigmoid(crowdRatio, steepness, 1.0);
    }

    /// <summary>
    /// Normalizes a collection of values to sum to 1 (softmax for probabilities).
    /// </summary>
    /// <param name="values">The values to normalize.</param>
    /// <returns>Normalized values that sum to 1.</returns>
    public static IReadOnlyList<double> Softmax(IEnumerable<double> values)
    {
        var valueList = values.ToList();
        if (valueList.Count == 0)
            return [];

        var expValues = valueList.Select(Math.Exp).ToList();
        var sumExp = expValues.Sum();

        return sumExp == 0
            ? valueList.Select(_ => 1.0 / valueList.Count).ToList()
            : expValues.Select(exp => exp / sumExp).ToList();
    }

    /// <summary>
    /// Linearly normalizes a value to the range [0, 1].
    /// </summary>
    /// <param name="value">The value to normalize.</param>
    /// <param name="min">Minimum value in the range.</param>
    /// <param name="max">Maximum value in the range.</param>
    /// <returns>Normalized value between 0 and 1.</returns>
    public static double LinearNormalize(double value, double min, double max)
    {
        if (max <= min)
            return .0;

        var clamped = Math.Clamp(value, min, max);
        return (clamped - min) / (max - min);
    }
}