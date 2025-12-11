using System.Diagnostics;

namespace dotMigrata.Simulation.Models;

/// <summary>
/// Tracks performance metrics for a simulation execution.
/// </summary>
/// <remarks>
/// Provides detailed timing and resource usage information for performance analysis and optimization.
/// </remarks>
public sealed class PerformanceMetrics
{
    private readonly List<TimeSpan> _tickDurations = [];
    private readonly Stopwatch _tickStopwatch = new();
    private readonly Stopwatch _totalStopwatch = new();
    private long _lastGcMemory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceMetrics" /> class.
    /// </summary>
    public PerformanceMetrics()
    {
        _lastGcMemory = GC.GetTotalMemory(false);
    }

    /// <summary>
    /// Gets the total elapsed time since simulation start.
    /// </summary>
    public TimeSpan TotalElapsed => _totalStopwatch.Elapsed;

    /// <summary>
    /// Gets the elapsed time for the current tick.
    /// </summary>
    public TimeSpan CurrentTickElapsed => _tickStopwatch.Elapsed;

    /// <summary>
    /// Gets the average duration per tick.
    /// </summary>
    public TimeSpan AverageTickDuration =>
        _tickDurations.Count > 0
            ? TimeSpan.FromTicks((long)_tickDurations.Average(t => t.Ticks))
            : TimeSpan.Zero;

    /// <summary>
    /// Gets the minimum tick duration observed.
    /// </summary>
    public TimeSpan MinTickDuration =>
        _tickDurations.Count > 0
            ? TimeSpan.FromTicks(_tickDurations.Min(t => t.Ticks))
            : TimeSpan.Zero;

    /// <summary>
    /// Gets the maximum tick duration observed.
    /// </summary>
    public TimeSpan MaxTickDuration =>
        _tickDurations.Count > 0
            ? TimeSpan.FromTicks(_tickDurations.Max(t => t.Ticks))
            : TimeSpan.Zero;

    /// <summary>
    /// Gets the current memory usage in bytes.
    /// </summary>
    public static long CurrentMemoryBytes => GC.GetTotalMemory(false);

    /// <summary>
    /// Gets the memory delta since last measurement in bytes.
    /// </summary>
    public long MemoryDeltaBytes
    {
        get
        {
            var current = CurrentMemoryBytes;
            var delta = current - _lastGcMemory;
            _lastGcMemory = current;
            return delta;
        }
    }

    /// <summary>
    /// Gets the total number of ticks executed.
    /// </summary>
    public int TotalTicks => _tickDurations.Count;

    /// <summary>
    /// Gets the average ticks per second based on total elapsed time.
    /// </summary>
    public double TicksPerSecond =>
        TotalElapsed.TotalSeconds > 0
            ? TotalTicks / TotalElapsed.TotalSeconds
            : 0;

    /// <summary>
    /// Starts timing the overall simulation.
    /// </summary>
    internal void StartSimulation()
    {
        _totalStopwatch.Restart();
        _lastGcMemory = GC.GetTotalMemory(false);
    }

    /// <summary>
    /// Stops timing the overall simulation.
    /// </summary>
    internal void StopSimulation()
    {
        _totalStopwatch.Stop();
        _tickStopwatch.Stop();
    }

    /// <summary>
    /// Starts timing a new tick.
    /// </summary>
    internal void StartTick()
    {
        _tickStopwatch.Restart();
    }

    /// <summary>
    /// Completes timing the current tick and records the duration.
    /// </summary>
    internal void CompleteTick()
    {
        _tickStopwatch.Stop();
        _tickDurations.Add(_tickStopwatch.Elapsed);
    }

    /// <summary>
    /// Gets a summary of performance metrics.
    /// </summary>
    /// <returns>A formatted string containing performance statistics.</returns>
    public string GetSummary()
    {
        return $"Performance: {TotalTicks} ticks in {TotalElapsed:g} " +
               $"(Avg: {AverageTickDuration.TotalMilliseconds:F2}ms/tick, " +
               $"Rate: {TicksPerSecond:F2} ticks/sec, " +
               $"Memory: {CurrentMemoryBytes / 1024.0 / 1024.0:F2} MB)";
    }

    /// <summary>
    /// Gets detailed performance statistics.
    /// </summary>
    /// <returns>A dictionary of metric names and values.</returns>
    public Dictionary<string, object> GetDetailedMetrics()
    {
        return new Dictionary<string, object>
        {
            ["TotalElapsed"] = TotalElapsed,
            ["TotalTicks"] = TotalTicks,
            ["AverageTickDuration"] = AverageTickDuration,
            ["MinTickDuration"] = MinTickDuration,
            ["MaxTickDuration"] = MaxTickDuration,
            ["TicksPerSecond"] = TicksPerSecond,
            ["CurrentMemoryMB"] = CurrentMemoryBytes / 1024.0 / 1024.0,
            ["MemoryDeltaKB"] = MemoryDeltaBytes / 1024.0
        };
    }
}