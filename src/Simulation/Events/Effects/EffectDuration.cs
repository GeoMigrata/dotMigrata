namespace dotMigrata.Simulation.Events.Effects;

/// <summary>
/// Defines the duration over which an effect is applied.
/// </summary>
public sealed class EffectDuration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EffectDuration" /> class.
    /// </summary>
    /// <param name="ticks">
    /// The number of ticks over which to apply the effect.
    /// If null, the effect is applied instantly in a single tick.
    /// </param>
    public EffectDuration(int? ticks)
    {
        Ticks = ticks;
    }

    /// <summary>
    /// Gets the number of ticks for this duration, or null for instant effects.
    /// </summary>
    public int? Ticks { get; }

    /// <summary>
    /// Gets a duration representing an instant, one-time effect.
    /// </summary>
    public static EffectDuration Instant { get; } = new(null);

    /// <summary>
    /// Creates a duration spanning the specified number of ticks.
    /// </summary>
    /// <param name="ticks">The number of ticks for the duration.</param>
    /// <returns>An <see cref="EffectDuration" /> representing the specified duration.</returns>
    public static EffectDuration Over(int ticks)
    {
        return new EffectDuration(ticks);
    }
}