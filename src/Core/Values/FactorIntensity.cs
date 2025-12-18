using dotMigrata.Core.Values.Attributes;

namespace dotMigrata.Core.Values;

/// <summary>
/// Represents the intensity value for a specific factor in a city.
/// Each city has a FactorIntensity for every FactorDefinition in the world.
/// </summary>
/// <remarks>
///     <para>
///     The intensity is stored as a <see cref="ValueSpec"/> which can be materialized once
///     before simulation begins for optimal runtime performance. This allows flexible value
///     specification during setup while maintaining minimal overhead during simulation.
///     </para>
///     <para>
///     Intensity values must always be non-negative (≥ 0). The sign of the factor's effect
///     is determined by the <see cref="FactorDefinition.Type" /> property, not by the intensity value.
///     </para>
/// </remarks>
public sealed record FactorIntensity
{
    private double? _materializedValue;

    /// <summary>
    /// Gets or initializes the factor definition this intensity is for.
    /// </summary>
    public required FactorDefinition Definition { get; init; }

    /// <summary>
    /// Gets or sets the intensity value specification of the factor.
    /// This specification should be materialized before simulation begins.
    /// Must produce non-negative values (>= 0).
    /// </summary>
    /// <remarks>
    /// The ValueSpec provides type-safe, convenient value specification during setup.
    /// Call <see cref="Materialize"/> before simulation to optimize runtime performance.
    /// </remarks>
    [ValueRange(0, double.MaxValue, AllowNegative = false)]
    public required ValueSpec Intensity { get; set; }

    /// <summary>
    /// Materializes the intensity value from the specification.
    /// Should be called once before simulation begins for optimal performance.
    /// </summary>
    /// <param name="random">Optional Random instance for value generation.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the evaluated value is negative.
    /// </exception>
    public void Materialize(Random? random = null)
    {
        var value = Intensity.Evaluate(null, false, random);

        if (value < 0)
            throw new InvalidOperationException(
                $"Factor intensity must be non-negative. Got: {value} for factor '{Definition.DisplayName}'. " +
                "Ensure the ValueSpec produces non-negative values.");

        _materializedValue = value;
    }

    /// <summary>
    /// Gets the intensity value. Returns materialized value if available, otherwise evaluates the spec.
    /// </summary>
    /// <param name="random">Optional Random instance for value generation if not materialized.</param>
    /// <returns>The intensity value (always non-negative).</returns>
    /// <remarks>
    /// For best performance, call <see cref="Materialize" /> before accessing this property repeatedly.
    /// </remarks>
    public double GetIntensity(Random? random = null)
    {
        if (_materializedValue.HasValue)
            return _materializedValue.Value;

        // Fallback: evaluate with caching
        var value = Intensity.Evaluate(null, true, random);

        if (value < 0)
            throw new InvalidOperationException(
                $"Factor intensity must be non-negative. Got: {value} for factor '{Definition.DisplayName}'. " +
                "Ensure the ValueSpec produces non-negative values.");

        return value;
    }

    /// <summary>
    /// Computes and returns the concrete intensity value from the specification.
    /// </summary>
    /// <param name="normalizedInput">
    /// Optional normalized input value (0-1 range) for deterministic evaluation.
    /// If null, the value is generated according to the specification's configuration.
    /// </param>
    /// <param name="useCache">
    /// When true, uses cached value if available. Default is true for better performance.
    /// </param>
    /// <param name="random">
    /// Optional Random instance for value generation. If null, uses a new Random instance.
    /// </param>
    /// <returns>The computed intensity value (always non-negative).</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the evaluated value is negative.
    /// </exception>
    /// <remarks>
    /// This method is retained for backward compatibility. Consider using <see cref="GetIntensity"/> instead.
    /// </remarks>
    public double ComputeIntensity(double? normalizedInput = null, bool useCache = true, Random? random = null)
    {
        // If materialized and no custom input, return materialized value
        if (_materializedValue.HasValue && normalizedInput == null)
            return _materializedValue.Value;

        var value = Intensity.Evaluate(normalizedInput, useCache, random);

        if (value < 0)
            throw new InvalidOperationException(
                $"Factor intensity must be non-negative. Got: {value} for factor '{Definition.DisplayName}'. " +
                "Ensure the ValueSpec produces non-negative values.");

        return value;
    }

    /// <summary>
    /// Normalizes the intensity value using the factor's normalization rules.
    /// </summary>
    /// <param name="factorDefinition">The factor definition containing normalization rules.</param>
    /// <param name="random">Optional Random instance for value generation.</param>
    /// <returns>Normalized value between 0 and 1.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factorDefinition" /> is null.
    /// </exception>
    internal double Normalize(FactorDefinition factorDefinition, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(factorDefinition);
        var intensity = GetIntensity(random);
        return factorDefinition.Normalize(intensity);
    }
}