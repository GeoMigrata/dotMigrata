using dotMigrata.Core.Exceptions;

namespace dotMigrata.Core.Values;

/// <summary>
/// Specification for generating <see cref="UnitValue" /> values in generators.
/// Supports fixed values, random ranges, and approximate distributions with lazy evaluation.
/// </summary>
/// <remarks>
/// This type is used exclusively in generators where values need to be evaluated lazily.
/// For immediate values (e.g., manual city setup), use <see cref="UnitValue" /> directly.
/// </remarks>
public sealed class UnitValuePromise
{
    /// <summary>
    /// Delegate for transform functions that map normalized input to output range.
    /// </summary>
    /// <param name="normalizedInput">Input value in [0, 1] range.</param>
    /// <param name="min">Minimum value of the output range.</param>
    /// <param name="max">Maximum value of the output range.</param>
    /// <returns>Transformed value within the specified range.</returns>
    public delegate double TransformFunc(double normalizedInput, double min, double max);

    private readonly double? _fixedValue;
    private readonly double? _mean;
    private readonly (double Min, double Max)? _range;
    private readonly double? _standardDeviation;
    private readonly TransformFunc? _transform;
    private UnitValue? _cached;

    private UnitValuePromise(
        double? fixedValue = null,
        (double, double)? range = null,
        double? mean = null,
        double? standardDeviation = null,
        TransformFunc? transform = null)
    {
        _fixedValue = fixedValue;
        _range = range;
        _mean = mean;
        _standardDeviation = standardDeviation;
        _transform = transform;
    }

    #region Transform Methods

    /// <summary>
    /// Applies a transform function to this specification.
    /// </summary>
    /// <param name="transform">The transform function to apply.</param>
    /// <returns>A new specification with the transform applied.</returns>
    /// <exception cref="ArgumentNullException">Thrown when transform is null.</exception>
    public UnitValuePromise WithTransform(TransformFunc transform)
    {
        ArgumentNullException.ThrowIfNull(transform);
        return new UnitValuePromise(_fixedValue, _range, _mean, _standardDeviation, transform);
    }

    #endregion

    #region Built-in Transforms

    /// <summary>
    /// Provides built-in transform functions.
    /// </summary>
    public static class Transforms
    {
        /// <summary>
        /// Linear transformation - proportional mapping from input to output range.
        /// </summary>
        public static TransformFunc Linear => (value, min, max) =>
        {
            var range = max - min;
            return range == 0 ? min : min + value * range;
        };

        /// <summary>
        /// Logarithmic transformation - emphasizes differences at lower values.
        /// </summary>
        public static TransformFunc Logarithmic => (value, min, max) =>
        {
            const double delta = 1e-6; // small offset to avoid log(0)
            var range = max - min;
            if (range == 0) return min;

            var numerator = Math.Log(value - min + delta);
            var denominator = Math.Log(range + delta);
            var normalized = denominator != 0 ? numerator / denominator : 0.0;

            return min + normalized * range;
        };

        /// <summary>
        /// Sigmoid (S-curve) transformation - smooth transition with emphasis on middle range.
        /// </summary>
        public static TransformFunc Sigmoid => (value, min, max) =>
        {
            const double steepness = 10.0;
            var range = max - min;
            if (range == 0) return min;

            var linear = (value - min) / range;
            var centered = (linear - 0.5) * steepness;
            var sigmoid = 1.0 / (1.0 + Math.Exp(-centered));

            return min + sigmoid * range;
        };

        /// <summary>
        /// Exponential transformation - emphasizes differences at higher values.
        /// </summary>
        public static TransformFunc Exponential => (value, min, max) =>
        {
            var range = max - min;
            if (range == 0) return min;

            var expValue = Math.Exp(value) - 1.0;
            const double expMax = Math.E - 1.0;
            var normalized = expValue / expMax;

            return min + normalized * range;
        };

        /// <summary>
        /// Square root transformation - moderate emphasis on lower values.
        /// </summary>
        public static TransformFunc SquareRoot => (value, min, max) =>
        {
            var range = max - min;
            if (range == 0) return min;

            var sqrtValue = Math.Sqrt(value);
            return min + sqrtValue * range;
        };
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a specification for a fixed value.
    /// </summary>
    /// <param name="value">The fixed value (will be clamped to [0, 1]).</param>
    public static UnitValuePromise Fixed(double value)
    {
        return new UnitValuePromise(value);
    }

    /// <summary>
    /// Creates a specification for a random value in the specified range.
    /// </summary>
    /// <param name="min">Minimum value (inclusive).</param>
    /// <param name="max">Maximum value (inclusive).</param>
    /// <exception cref="GeneratorSpecificationException">Thrown when min > max.</exception>
    public static UnitValuePromise InRange(double min, double max)
    {
        if (min > max)
            throw new GeneratorSpecificationException(
                $"Minimum value ({min}) must be less than or equal to maximum value ({max}).");

        return new UnitValuePromise(range: (min, max));
    }

    /// <summary>
    /// Creates a specification for an approximate value using normal distribution.
    /// </summary>
    /// <param name="mean">The mean value.</param>
    /// <param name="standardDeviation">The standard deviation (must be positive).</param>
    /// <exception cref="GeneratorSpecificationException">Thrown when standard deviation is not positive.</exception>
    public static UnitValuePromise Approximately(double mean, double standardDeviation)
    {
        if (standardDeviation <= 0)
            throw new GeneratorSpecificationException(
                $"Standard deviation must be positive. Got: {standardDeviation}");

        return new UnitValuePromise(mean: mean, standardDeviation: standardDeviation);
    }

    #endregion

    #region Evaluation

    /// <summary>
    /// Evaluates this specification to produce a concrete <see cref="UnitValue" /> value.
    /// </summary>
    /// <param name="random">Optional Random instance for value generation. If null, uses a new instance.</param>
    /// <param name="useCache">When true, returns cached value if available. Default is false.</param>
    /// <returns>The evaluated normalized value.</returns>
    public UnitValue Evaluate(Random? random = null, bool useCache = false)
    {
        // Return cached value if requested and available
        if (useCache && _cached.HasValue)
            return _cached.Value;

        // Fixed values are always returned as-is
        if (_fixedValue.HasValue)
        {
            var result = UnitValue.FromRatio(_fixedValue.Value);
            if (useCache)
                _cached = result;
            return result;
        }

        // Compute the value
        var value = ComputeValue(random);

        if (useCache)
            _cached = value;

        return value;
    }

    /// <summary>
    /// Invalidates the cached value, forcing re-evaluation on next call.
    /// </summary>
    public void InvalidateCache()
    {
        _cached = null;
    }

    private UnitValue ComputeValue(Random? random)
    {
        double rawValue;

        // Handle approximate (normal distribution)
        if (_mean.HasValue && _standardDeviation.HasValue)
        {
            rawValue = GenerateNormalRandom(_mean.Value, _standardDeviation.Value, random);
        }

        // Handle ranged values
        else if (_range.HasValue)
        {
            var (min, max) = _range.Value;
            rawValue = GenerateUniformRandom(min, max, random);

            // Apply transform if specified
            if (_transform == null)
                return UnitValue.FromRatio(rawValue);

            var normalized = (rawValue - min) / (max - min);
            rawValue = _transform(normalized, min, max);
        }
        else
        {
            throw new InvalidOperationException(
                "Specification must have either a fixed value, a range, or approximate distribution.");
        }

        return UnitValue.FromRatio(rawValue);
    }

    private static double GenerateUniformRandom(double min, double max, Random? random)
    {
        random ??= new Random();
        return min + random.NextDouble() * (max - min);
    }

    private static double GenerateNormalRandom(double mean, double stdDev, Random? random)
    {
        random ??= new Random();

        // Box-Muller transform for normal distribution
        var u1 = 1.0 - random.NextDouble();
        var u2 = 1.0 - random.NextDouble();
        var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

        return mean + stdDev * randStdNormal;
    }

    #endregion
}