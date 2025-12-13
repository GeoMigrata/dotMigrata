using System.Diagnostics;
using System.Runtime.CompilerServices;
using dotMigrata.Core.Entities;
using dotMigrata.Core.Values;

namespace dotMigrata.Logic.Feedback;

/// <summary>
/// Feedback strategy that adjusts economic factors based on per-capita metrics.
/// </summary>
/// <remarks>
/// This strategy models how population density affects economic indicators.
/// As population increases relative to capacity, economic factors may change.
/// </remarks>
[DebuggerDisplay("Feedback: {Name}")]
public sealed class PerCapitaFeedbackStrategy : IFeedbackStrategy
{
    private readonly FactorDefinition _factor;
    private readonly double _minimumPopulation;
    private readonly double _scalingFactor;

    /// <summary>
    /// Initializes a new instance of the PerCapitaFeedbackStrategy.
    /// </summary>
    /// <param name="factor">The factor definition to adjust (e.g., "Economy", "Infrastructure").</param>
    /// <param name="scalingFactor">The scaling factor for population impact (default: 0.01).</param>
    /// <param name="minimumPopulation">Minimum population required to apply feedback (default: 100).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factor" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when scalingFactor or minimumPopulation is negative.</exception>
    public PerCapitaFeedbackStrategy(
        FactorDefinition factor,
        double scalingFactor = 0.01,
        double minimumPopulation = 100)
    {
        ArgumentNullException.ThrowIfNull(factor);

        if (scalingFactor < 0)
            throw new ArgumentOutOfRangeException(nameof(scalingFactor), "Scaling factor must be non-negative.");
        if (minimumPopulation < 0)
            throw new ArgumentOutOfRangeException(nameof(minimumPopulation),
                "Minimum population must be non-negative.");

        _factor = factor;
        _scalingFactor = scalingFactor;
        _minimumPopulation = minimumPopulation;
    }

    /// <inheritdoc />
    public string Name => $"PerCapita({_factor.DisplayName})";

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldApply(City city, World world)
    {
        return city.Population >= _minimumPopulation;
    }

    /// <inheritdoc />
    public void ApplyFeedback(City city, World world)
    {
        if (!city.TryGetFactorValue(_factor, out var currentFactor)) return;

        // Calculate per-capita adjustment
        var populationRatio = city.Population / Math.Max(city.Capacity ?? city.Population, 1.0);
        var adjustment = populationRatio * _scalingFactor;

        // Apply adjustment with clamping to valid range
        var newIntensity = Math.Clamp(
            currentFactor.Intensity.Value + adjustment,
            _factor.MinValue,
            _factor.MaxValue
        );

        city.UpdateFactorIntensity(_factor, IntensityValue.FromRaw(newIntensity));
    }
}