using Microsoft.Extensions.Logging;

namespace dotMigrata.Simulation.Logging;

/// <summary>
/// Defines logging event IDs for simulation operations.
/// </summary>
/// <remarks>
/// Event IDs are organized by category for filtering and diagnostics.
/// </remarks>
public static class SimulationEvents
{
    // Lifecycle Events (1000-1999)
    /// <summary>Event ID for simulation start.</summary>
    public static readonly EventId SimulationStarted = new(1000, nameof(SimulationStarted));

    /// <summary>Event ID for simulation completion.</summary>
    public static readonly EventId SimulationCompleted = new(1001, nameof(SimulationCompleted));

    /// <summary>Event ID for simulation cancellation.</summary>
    public static readonly EventId SimulationCancelled = new(1002, nameof(SimulationCancelled));

    /// <summary>Event ID for simulation errors.</summary>
    public static readonly EventId SimulationError = new(1003, nameof(SimulationError));

    // Tick Events (2000-2999)
    /// <summary>Event ID for tick start.</summary>
    public static readonly EventId TickStarted = new(2000, nameof(TickStarted));

    /// <summary>Event ID for tick completion.</summary>
    public static readonly EventId TickCompleted = new(2001, nameof(TickCompleted));

    // Stage Events (3000-3999)
    /// <summary>Event ID for stage execution.</summary>
    public static readonly EventId StageExecuted = new(3000, nameof(StageExecuted));

    /// <summary>Event ID for stage errors.</summary>
    public static readonly EventId StageError = new(3001, nameof(StageError));

    // Stability Events (4000-4999)
    /// <summary>Event ID for stability check.</summary>
    public static readonly EventId StabilityChecked = new(4000, nameof(StabilityChecked));

    /// <summary>Event ID for stability achieved.</summary>
    public static readonly EventId StabilityAchieved = new(4001, nameof(StabilityAchieved));

    // Migration Events (5000-5999)
    /// <summary>Event ID for migration decisions.</summary>
    public static readonly EventId MigrationDecisions = new(5000, nameof(MigrationDecisions));

    /// <summary>Event ID for migration executions.</summary>
    public static readonly EventId MigrationExecutions = new(5001, nameof(MigrationExecutions));

    // Performance Events (6000-6999)
    /// <summary>Event ID for performance metrics.</summary>
    public static readonly EventId PerformanceMetrics = new(6000, nameof(PerformanceMetrics));

    /// <summary>Event ID for memory usage.</summary>
    public static readonly EventId MemoryUsage = new(6001, nameof(MemoryUsage));
}