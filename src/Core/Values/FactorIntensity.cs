using dotMigrata.Core.Values.Attributes;

namespace dotMigrata.Core.Values;

/// <summary>
/// Represents the intensity value for a specific factor in a city.
/// Each city has a FactorIntensity for every FactorDefinition in the world.
/// </summary>
/// <remarks>
///     <para>
///     The intensity is stored as a <see cref="ValueSpec" /> which is evaluated lazily
///     when needed for calculations. This allows flexible value specification including
///     fixed values, random ranges, or approximate distributions.
///     </para>
///     <para>
///     Intensity values must always be non-negative (≥ 0). The sign of the factor's effect
///     is determined by the <see cref="FactorDefinition.Type" /> property, not by the intensity value.
///     </para>
/// </remarks>
public sealed record FactorIntensity
{
    /// <summary>
    /// Gets or initializes the factor definition this intensity is for.
    /// </summary>
    public required FactorDefinition Definition { get; init; }

    /// <summary>
    /// Gets or sets the intensity value specification of the factor.
    /// This specification will be evaluated to produce the actual intensity value during calculations.
    /// Must produce non-negative values (>= 0).
    /// </summary>
    /// <remarks>
    /// The ValueSpec is evaluated on-demand, allowing for lazy computation and flexible value generation.
    /// Use <see cref="ComputeIntensity" /> to obtain the concrete intensity value.
    /// </remarks>
    [ValueRange(0, double.MaxValue, AllowNegative = false)]
    public required ValueSpec Intensity { get; set; }

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
    public double ComputeIntensity(double? normalizedInput = null, bool useCache = true, Random? random = null)
    {
        var value = Intensity.Evaluate(normalizedInput, useCache, random);

        // Only validate when not using cached values or when no cache exists
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
    /// <param name="normalizedInput">
    /// Optional normalized input value (0-1 range) for deterministic evaluation.
    /// </param>
    /// <param name="useCache">When true, uses cached value if available.</param>
    /// <param name="random">Optional Random instance for value generation.</param>
    /// <returns>Normalized value between 0 and 1.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factorDefinition" /> is null.
    /// </exception>
    internal double Normalize(FactorDefinition factorDefinition, double? normalizedInput = null,
        bool useCache = false, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(factorDefinition);
        var intensity = ComputeIntensity(normalizedInput, useCache, random);
        return factorDefinition.Normalize(intensity);
    }
}