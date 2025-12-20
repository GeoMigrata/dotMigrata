using System.Diagnostics;
using System.Runtime.CompilerServices;
using dotMigrata.Core.Entities;
using dotMigrata.Core.Values;

namespace dotMigrata.Logic.Feedback;

/// <summary>
/// Feedback strategy that models congestion effects on city factors.
/// </summary>
/// <remarks>
/// This strategy reduces certain factors (e.g., quality of life, infrastructure)
/// when population approaches or exceeds capacity.
/// </remarks>
[DebuggerDisplay("Feedback: {Name}")]
public sealed class CongestionFeedbackStrategy : IFeedbackStrategy
{
    private readonly double _congestionThreshold;
    private readonly FactorDefinition _factor;
    private readonly double _impactStrength;

    /// <summary>
    /// Initializes a new instance of the CongestionFeedbackStrategy.
    /// </summary>
    /// <param name="factor">The factor definition affected by congestion.</param>
    /// <param name="congestionThreshold">Population ratio threshold for congestion (default: 0.8).</param>
    /// <param name="impactStrength">Strength of congestion impact (default: 0.05).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factor" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when parameters are out of valid range.</exception>
    public CongestionFeedbackStrategy(
        FactorDefinition factor,
        double congestionThreshold = 0.8,
        double impactStrength = 0.05)
    {
        ArgumentNullException.ThrowIfNull(factor);

        if (congestionThreshold is <= 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(congestionThreshold), "Threshold must be between 0 and 1.");
        if (impactStrength < 0)
            throw new ArgumentOutOfRangeException(nameof(impactStrength), "Impact strength must be non-negative.");

        _factor = factor;
        _congestionThreshold = congestionThreshold;
        _impactStrength = impactStrength;
    }

    /// <inheritdoc />
    public string Name => $"Congestion({_factor.DisplayName})";

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldApply(City city, World world)
    {
        if (city.Capacity == null) return false;
        var ratio = city.GetPopulationRatio();
        return ratio >= _congestionThreshold;
    }

    /// <inheritdoc />
    public void ApplyFeedback(City city, World world)
    {
        if (!city.TryGetFactorIntensity(_factor, out var currentIntensity)) return;

        if (city.Capacity == null) return;

        // Calculate congestion severity using consolidated helper
        var populationRatio = city.GetPopulationRatio();
        var congestionSeverity = Math.Max(0, populationRatio - _congestionThreshold);

        // Apply negative adjustment (congestion reduces factor)
        var reduction = congestionSeverity * _impactStrength;
        var currentValue = (double)currentIntensity.Value;
        var newIntensity = Math.Max(currentValue - reduction, 0.0);

        var updatedIntensity = new FactorIntensity { Definition = _factor, Value = UnitValue.FromRatio(newIntensity) };
        city.UpdateFactorIntensity(updatedIntensity);
    }
}