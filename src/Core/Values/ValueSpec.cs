using dotMigrata.Core.Exceptions;

namespace dotMigrata.Core.Values;

/// <summary>
/// Represents a value specification that can be evaluated to produce concrete values.
/// Supports fixed values, random ranges, approximate distributions, and transform functions.
/// </summary>
/// <remarks>
///     <para>
///     ValueSpec instances store the specification of how a value should be generated or computed,
///     not the value itself. Actual values are computed lazily via the <see cref="Evaluate" /> method.
///     </para>
///     <para>
///     This type supports caching to optimize repeated evaluations and transform delegates
///     for flexible value transformation.
///     </para>
/// </remarks>
public sealed record ValueSpec
{
    private readonly double? _fixedValue;
    private readonly double? _mean;
    private readonly (double Min, double Max)? _range;
    private readonly double? _standardDeviation;

    // Cache for evaluated values (mutable field in immutable record)
    private double? _cachedValue;
    private bool _isCached;

    private ValueSpec(
        double? fixedValue,
        (double, double)? range,
        double scale,
        double? mean,
        double? standardDeviation,
        TransformDelegate? transform)
    {
        _fixedValue = fixedValue;
        _range = range;
        Scale = scale;
        _mean = mean;
        _standardDeviation = standardDeviation;
        Transform = transform;
    }

    /// <summary>
    /// Delegate type for transform functions that map values from one range to another.
    /// </summary>
    /// <param name="value">The input value to transform.</param>
    /// <param name="min">The minimum value of the output range.</param>
    /// <param name="max">The maximum value of the output range.</param>
    /// <returns>The transformed value within the specified range.</returns>
    public delegate double TransformDelegate(double value, double min, double max);

    /// <summary>
    /// Gets whether this is a fixed value specification.
    /// </summary>
    public bool IsFixed => _fixedValue.HasValue;

    /// <summary>
    /// Gets whether this has a custom range.
    /// </summary>
    public bool HasRange => _range.HasValue;

    /// <summary>
    /// Gets whether this is an approximate value using normal distribution.
    /// </summary>
    public bool IsApproximate => _mean.HasValue && _standardDeviation.HasValue;

    /// <summary>
    /// Gets the fixed value if this is a fixed specification.
    /// </summary>
    public double? FixedValue => _fixedValue;

    /// <summary>
    /// Gets the range if this has a custom range.
    /// </summary>
    public (double Min, double Max)? Range => _range;

    /// <summary>
    /// Gets the scale factor to apply to generated values.
    /// Values greater than 1.0 tend toward higher values, less than 1.0 toward lower values.
    /// </summary>
    public double Scale { get; init; }

    /// <summary>
    /// Gets the mean value for approximate specifications.
    /// </summary>
    public double? Mean => _mean;

    /// <summary>
    /// Gets the standard deviation for approximate specifications.
    /// </summary>
    public double? StandardDeviation => _standardDeviation;

    /// <summary>
    /// Gets the transform function applied during evaluation, if any.
    /// </summary>
    public TransformDelegate? Transform { get; }

    /// <summary>
    /// Evaluates this specification to produce a concrete value.
    /// </summary>
    /// <param name="normalizedInput">
    /// Optional normalized input value (0-1 range). If provided, uses this value
    /// instead of generating a random value. Useful for deterministic evaluation.
    /// </param>
    /// <param name="useCache">
    /// When true, returns cached value if available (only for evaluations without normalizedInput).
    /// Default is false.
    /// </param>
    /// <param name="random">
    /// Optional Random instance for value generation. If null, uses a new Random instance.
    /// </param>
    /// <returns>The evaluated concrete value.</returns>
    /// <remarks>
    /// For fixed values, always returns the fixed value.
    /// For ranged values, applies transform if specified.
    /// Caching only applies when normalizedInput is null and useCache is true.
    /// </remarks>
    public double Evaluate(double? normalizedInput = null, bool useCache = false, Random? random = null)
    {
        // Fixed values are always returned as-is
        if (_fixedValue.HasValue)
            return _fixedValue.Value;

        // Check cache if requested and no specific input provided
        if (useCache && _isCached && normalizedInput == null)
            return _cachedValue!.Value;

        // Compute the value
        var value = ComputeValue(normalizedInput, random);

        // Cache if requested and no specific input
        if (!useCache || normalizedInput != null)
            return value;

        _cachedValue = value;
        _isCached = true;

        return value;
    }

    /// <summary>
    /// Invalidates the cached value, forcing re-evaluation on next call.
    /// </summary>
    /// <returns>This instance for fluent chaining.</returns>
    public ValueSpec InvalidateCache()
    {
        _isCached = false;
        _cachedValue = null;
        return this;
    }

    private double ComputeValue(double? normalizedInput, Random? random)
    {
        double baseValue;

        // Handle approximate (normal distribution)
        if (IsApproximate)
        {
            baseValue = GenerateNormalRandom(_mean!.Value, _standardDeviation!.Value, random);
        }
        // Handle ranged values
        else if (HasRange)
        {
            var (min, max) = _range!.Value;

            // Use provided normalized input (0-1 range)
            baseValue = normalizedInput.HasValue
                ? ApplyTransform(normalizedInput.Value, min, max)
                : GenerateUniformRandom(min, max, random); // Generate random value in range

            // Note: For random generation, transform is applied in GenerateUniformRandom
            // to maintain backward compatibility with existing behavior
        }
        else
        {
            // No range specified - this shouldn't happen in normal usage
            throw new InvalidOperationException(
                "Value specification must have either a fixed value, a range, or approximate distribution.");
        }

        // Apply scale
        return baseValue * Scale;
    }

    private double ApplyTransform(double normalizedValue, double min, double max)
    {
        if (Transform != null)
            return Transform(normalizedValue, min, max);

        // Default linear mapping
        return min + normalizedValue * (max - min);
    }

    private static double GenerateUniformRandom(double min, double max, Random? random)
    {
        random ??= new Random();
        return min + random.NextDouble() * (max - min);
    }

    private static double GenerateNormalRandom(double mean, double stdDev, Random? random)
    {
        random ??= new Random();

        // Box-Muller transform
        var u1 = 1.0 - random.NextDouble();
        var u2 = 1.0 - random.NextDouble();
        var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

        return mean + stdDev * randStdNormal;
    }

    /// <summary>
    /// Creates a fixed value specification.
    /// </summary>
    /// <param name="value">The fixed value.</param>
    /// <returns>A value specification with a fixed value.</returns>
    public static ValueSpec Fixed(double value)
    {
        return new ValueSpec(value, null, 1.0, null, null, null);
    }

    /// <summary>
    /// Creates a random value specification with a range.
    /// </summary>
    /// <param name="min">Minimum value (inclusive).</param>
    /// <param name="max">Maximum value (inclusive). Must be greater than or equal to <paramref name="min" />.</param>
    /// <returns>A value specification with a random range.</returns>
    /// <exception cref="GeneratorSpecificationException">
    /// Thrown when <paramref name="min" /> is greater than <paramref name="max" />.
    /// </exception>
    public static ValueSpec InRange(double min, double max)
    {
        return min > max
            ? throw new GeneratorSpecificationException(
                $"Minimum value ({min}) must be less than or equal to maximum value ({max}).")
            : new ValueSpec(null, (min, max), 1.0, null, null, null);
    }

    /// <summary>
    /// Creates a value specification that represents an approximate value using a normal distribution.
    /// Values will be sampled from a normal distribution with the specified mean and standard deviation.
    /// </summary>
    /// <param name="mean">The mean value (center of the distribution).</param>
    /// <param name="standardDeviation">The standard deviation. Must be positive.</param>
    /// <returns>A value specification representing an approximate value.</returns>
    /// <exception cref="GeneratorSpecificationException">
    /// Thrown when <paramref name="standardDeviation" /> is not positive.
    /// </exception>
    public static ValueSpec Approximately(double mean, double standardDeviation)
    {
        return standardDeviation <= 0
            ? throw new GeneratorSpecificationException(
                $"Standard deviation must be positive. Got: {standardDeviation}")
            : new ValueSpec(null, null, 1.0, mean, standardDeviation, null);
    }

    /// <summary>
    /// Creates a random value specification with a scale.
    /// The scale affects the distribution: values greater than 1.0 bias toward higher values,
    /// less than 1.0 bias toward lower values.
    /// </summary>
    /// <param name="scale">Scale factor to apply to the random value. Must be non-negative.</param>
    /// <returns>A value specification with default range and scale.</returns>
    /// <exception cref="GeneratorSpecificationException">Thrown when <paramref name="scale" /> is negative.</exception>
    public static ValueSpec RandomWithScale(double scale)
    {
        return scale < 0
            ? throw new GeneratorSpecificationException($"Scale must be non-negative. Got: {scale}")
            : new ValueSpec(null, null, scale, null, null, null);
    }

    /// <summary>
    /// Creates a value specification for default random generation (no range, no scale).
    /// </summary>
    /// <returns>A value specification for default random generation.</returns>
    public static ValueSpec Random()
    {
        return new ValueSpec(null, null, 1.0, null, null, null);
    }

    /// <summary>
    /// Applies a scale to this specification, creating a biased random value.
    /// The scale affects the distribution: values greater than 1.0 bias toward higher values,
    /// less than 1.0 bias toward lower values.
    /// </summary>
    /// <param name="scale">Scale factor to multiply the generated value by. Must be non-negative.</param>
    /// <returns>A new value specification with the scale applied.</returns>
    /// <exception cref="GeneratorSpecificationException">Thrown when <paramref name="scale" /> is negative.</exception>
    public ValueSpec WithScale(double scale)
    {
        return scale < 0
            ? throw new GeneratorSpecificationException($"Scale must be non-negative. Got: {scale}")
            : this with { Scale = scale };
    }

    /// <summary>
    /// Applies a transform function to this specification.
    /// The transform is applied when evaluating the value within its range.
    /// </summary>
    /// <param name="transform">The transform delegate to apply.</param>
    /// <returns>A new value specification with the transform applied.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="transform" /> is null.</exception>
    public ValueSpec WithTransform(TransformDelegate transform)
    {
        ArgumentNullException.ThrowIfNull(transform);
        return new ValueSpec(_fixedValue, _range, Scale, _mean, _standardDeviation, transform);
    }

    /// <summary>
    /// Provides predefined transform delegates for common transformation patterns.
    /// </summary>
    public static class Transforms
    {
        /// <summary>
        /// Linear transformation - maps input linearly from 0-1 to min-max range.
        /// </summary>
        public static readonly TransformDelegate Linear = (value, min, max) =>
        {
            var range = max - min;
            return range == 0 ? min : min + value * range;
        };

        /// <summary>
        /// Logarithmic transformation - emphasizes differences at lower values.
        /// </summary>
        public static readonly TransformDelegate Logarithmic = (value, min, max) =>
        {
            const double delta = 1e-6; // small offset to avoid log(0)
            var range = max - min;
            if (range == 0) return min;

            var numerator = Math.Log(value + delta);
            var denominator = Math.Log(1.0 + delta);
            var normalized = denominator != 0 ? numerator / denominator : 0.0;

            return min + normalized * range;
        };

        /// <summary>
        /// Sigmoid (S-curve) transformation - smooth transition with emphasis on middle range.
        /// </summary>
        public static readonly TransformDelegate Sigmoid = (value, min, max) =>
        {
            const double steepness = 10.0;
            var range = max - min;
            if (range == 0) return min;

            var centered = (value - 0.5) * steepness;
            var sigmoid = 1.0 / (1.0 + Math.Exp(-centered));

            return min + sigmoid * range;
        };

        /// <summary>
        /// Exponential transformation - emphasizes differences at higher values.
        /// </summary>
        public static readonly TransformDelegate Exponential = (value, min, max) =>
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
        public static readonly TransformDelegate SquareRoot = (value, min, max) =>
        {
            var range = max - min;
            if (range == 0) return min;

            var sqrtValue = Math.Sqrt(value);
            return min + sqrtValue * range;
        };
    }
}