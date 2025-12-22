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
    private readonly List<TimeSpan> _stepDurations = [];
    private readonly Stopwatch _stepStopwatch = new();
    private readonly Stopwatch _totalStopwatch = new();
    private long _lastGcMemory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceMetrics" /> class.
    /// </summary>
    public PerformanceMetrics() => _lastGcMemory = GC.GetTotalMemory(false);

    /// <summary>
    /// Gets the total elapsed time since simulation start.
    /// </summary>
    public TimeSpan TotalElapsed => _totalStopwatch.Elapsed;

    /// <summary>
    /// Gets the elapsed time for the current step.
    /// </summary>
    public TimeSpan CurrentStepElapsed => _stepStopwatch.Elapsed;

    /// <summary>
    /// Gets the average duration per step.
    /// </summary>
    public TimeSpan AverageStepDuration =>
        _stepDurations.Count > 0
            ? TimeSpan.FromTicks((long)_stepDurations.Average(t => t.Ticks))
            : TimeSpan.Zero;

    /// <summary>
    /// Gets the minimum step duration observed.
    /// </summary>
    public TimeSpan MinStepDuration =>
        _stepDurations.Count > 0
            ? TimeSpan.FromTicks(_stepDurations.Min(t => t.Ticks))
            : TimeSpan.Zero;

    /// <summary>
    /// Gets the maximum step duration observed.
    /// </summary>
    public TimeSpan MaxStepDuration =>
        _stepDurations.Count > 0
            ? TimeSpan.FromTicks(_stepDurations.Max(t => t.Ticks))
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
    /// Gets the total number of steps executed.
    /// </summary>
    public int TotalSteps => _stepDurations.Count;

    /// <summary>
    /// Gets the average steps per second based on total elapsed time.
    /// </summary>
    public double StepsPerSecond =>
        TotalElapsed.TotalSeconds > 0
            ? TotalSteps / TotalElapsed.TotalSeconds
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
        _stepStopwatch.Stop();
    }

    /// <summary>
    /// Starts timing a new step.
    /// </summary>
    internal void StartStep() => _stepStopwatch.Restart();

    /// <summary>
    /// Completes timing the current step and records the duration.
    /// </summary>
    internal void CompleteStep()
    {
        _stepStopwatch.Stop();
        _stepDurations.Add(_stepStopwatch.Elapsed);
    }

    /// <summary>
    /// Gets a summary of performance metrics.
    /// </summary>
    /// <returns>A formatted string containing performance statistics.</returns>
    public string GetSummary() =>
        $"Performance: {TotalSteps} steps in {TotalElapsed:g} " +
        $"(Avg: {AverageStepDuration.TotalMilliseconds:F2}ms/step, " +
        $"Rate: {StepsPerSecond:F2} steps/sec, " +
        $"Memory: {CurrentMemoryBytes / 1024.0 / 1024.0:F2} MB)";

    /// <summary>
    /// Gets detailed performance statistics.
    /// </summary>
    /// <returns>A dictionary of metric names and values.</returns>
    public Dictionary<string, object> GetDetailedMetrics() =>
        new()
        {
            ["TotalElapsed"] = TotalElapsed,
            ["TotalStepSteps"] = TotalSteps,
            ["AverageStepDuration"] = AverageStepDuration,
            ["MinStepDuration"] = MinStepDuration,
            ["MaxStepDuration"] = MaxStepDuration,
            ["StepsPerSecond"] = StepsPerSecond,
            ["CurrentMemoryMB"] = CurrentMemoryBytes / 1024.0 / 1024.0,
            ["MemoryDeltaKB"] = MemoryDeltaBytes / 1024.0
        };
}