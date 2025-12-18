using System.Runtime.CompilerServices;
using dotMigrata.Core.Entities;
using dotMigrata.Core.Values;
using dotMigrata.Simulation.Events.Enums;
using dotMigrata.Simulation.Events.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Events.Effects;

/// <summary>
/// Effect that changes factor values for cities.
/// Supports various application types (absolute, delta, multiply, transitions)
/// and reuses <see cref="ValueSpecification" /> for flexible value generation.
/// </summary>
public sealed class FactorChangeEffect : IEventEffect
{
    private readonly Dictionary<City, FactorChangeState> _cityStates = [];
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the <see cref="FactorChangeEffect" /> class.
    /// </summary>
    /// <param name="factor">The factor definition to modify.</param>
    /// <param name="valueSpecification">
    /// Specification for the target value (fixed, range, or approximate).
    /// Leverages existing <see cref="ValueSpecification" /> infrastructure.
    /// </param>
    /// <param name="applicationType">How the value change is applied.</param>
    /// <param name="duration">Duration over which the effect is applied.</param>
    /// <param name="cityFilter">Optional predicate to filter which cities are affected.</param>
    /// <param name="seed">Optional random seed for reproducible value generation.</param>
    public FactorChangeEffect(
        FactorDefinition factor,
        ValueSpec valueSpecification,
        EffectApplicationType applicationType,
        EffectDuration duration,
        Func<City, bool>? cityFilter = null,
        int? seed = null)
    {
        Factor = factor ?? throw new ArgumentNullException(nameof(factor));
        ValueSpecification = valueSpecification ?? throw new ArgumentNullException(nameof(valueSpecification));
        ApplicationType = applicationType;
        Duration = duration ?? throw new ArgumentNullException(nameof(duration));
        CityFilter = cityFilter;
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    /// <summary>
    /// Gets the factor being modified.
    /// </summary>
    public FactorDefinition Factor { get; }

    /// <summary>
    /// Gets the value specification for target values.
    /// </summary>
    public ValueSpec ValueSpecification { get; }

    /// <summary>
    /// Gets the type of application for this effect.
    /// </summary>
    public EffectApplicationType ApplicationType { get; }

    /// <summary>
    /// Gets the duration over which this effect is applied.
    /// </summary>
    public EffectDuration Duration { get; }

    /// <summary>
    /// Gets the optional city filter predicate.
    /// </summary>
    public Func<City, bool>? CityFilter { get; }

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public void Apply(SimulationContext context)
    {
        var cities = context.World.Cities.Where(c => CityFilter?.Invoke(c) ?? true);

        foreach (var city in cities)
        {
            if (!city.TryGetFactorIntensity(Factor, out var currentIntensity))
                continue;

            var currentValue = currentIntensity.ComputeIntensity();
            var state = GetOrCreateState(city, currentValue, context.CurrentTick);
            var newValue = CalculateNewValue(state, currentValue, context.CurrentTick);

            newValue = Math.Clamp(newValue, Factor.MinValue, Factor.MaxValue);
            city.UpdateFactorIntensity(Factor, ValueSpec.Fixed(newValue));
        }
    }

    private FactorChangeState GetOrCreateState(City city, double currentValue, int tick)
    {
        if (_cityStates.TryGetValue(city, out var state))
            return state;

        state = new FactorChangeState
        {
            InitialValue = currentValue,
            StartTick = tick,
            TargetValue = GenerateTargetValue()
        };
        _cityStates[city] = state;

        return state;
    }

    private double GenerateTargetValue()
    {
        if (ValueSpecification.IsFixed)
            return ValueSpecification.FixedValue!.Value;

        if (ValueSpecification.HasRange)
        {
            var (min, max) = ValueSpecification.Range!.Value;
            return min + _random.NextDouble() * (max - min) * ValueSpecification.Scale;
        }

        if (!ValueSpecification.IsApproximate)
            return _random.NextDouble() * ValueSpecification.Scale;

        // Box-Muller transform for normal distribution
        var u1 = 1.0 - _random.NextDouble();
        var u2 = 1.0 - _random.NextDouble();
        var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return ValueSpecification.Mean!.Value +
               ValueSpecification.StandardDeviation!.Value * randStdNormal * ValueSpecification.Scale;

        // Default random [0,1]
    }

    private double CalculateNewValue(FactorChangeState state, double currentValue, int currentTick)
    {
        return ApplicationType switch
        {
            EffectApplicationType.Absolute => state.TargetValue,
            EffectApplicationType.Delta => currentValue + state.TargetValue,
            EffectApplicationType.Multiply => currentValue * state.TargetValue,
            EffectApplicationType.LinearTransition => CalculateLinearTransition(state, currentTick),
            EffectApplicationType.LogarithmicTransition => CalculateLogarithmicTransition(state, currentTick),
            _ => currentValue
        };
    }


    private double CalculateLinearTransition(FactorChangeState state, int currentTick)
    {
        if (!Duration.Ticks.HasValue)
            return state.TargetValue;

        var elapsed = currentTick - state.StartTick;
        var progress = Math.Min(1.0, elapsed / (double)Duration.Ticks.Value);

        return state.InitialValue + (state.TargetValue - state.InitialValue) * progress;
    }

    private double CalculateLogarithmicTransition(FactorChangeState state, int currentTick)
    {
        if (!Duration.Ticks.HasValue)
            return state.TargetValue;

        var elapsed = currentTick - state.StartTick;
        var totalDuration = Duration.Ticks.Value;

        // Logarithmic curve: fast change initially, then slower
        var progress = Math.Log(elapsed + 1) / Math.Log(totalDuration + 1);
        progress = Math.Min(1.0, progress);

        return state.InitialValue + (state.TargetValue - state.InitialValue) * progress;
    }

    private sealed class FactorChangeState
    {
        public required double InitialValue { get; init; }
        public required double TargetValue { get; init; }
        public required int StartTick { get; init; }
    }
}