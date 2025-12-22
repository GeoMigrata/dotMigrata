namespace dotMigrata.Simulation.Events.Effects;

/// <summary>
/// Defines the duration over which an effect is applied.
/// </summary>
public sealed class EffectDuration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EffectDuration" /> class.
    /// </summary>
    /// <param name="steps">
    /// The number of steps over which to apply the effect.
    /// If null, the effect is applied instantly in a single step.
    /// </param>
    public EffectDuration(int? steps) => Steps = steps;

    /// <summary>
    /// Gets the number of steps for this duration, or null for instant effects.
    /// </summary>
    public int? Steps { get; }

    /// <summary>
    /// Gets a duration representing an instant, one-time effect.
    /// </summary>
    public static EffectDuration Instant { get; } = new(null);

    /// <summary>
    /// Creates a duration spanning the specified number of steps.
    /// </summary>
    /// <param name="steps">The number of steps for the duration.</param>
    /// <returns>An <see cref="EffectDuration" /> representing the specified duration.</returns>
    public static EffectDuration Over(int steps) => new(steps);
}