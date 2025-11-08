using dotGeoMigrata.Simulation.Interfaces;
using dotGeoMigrata.Simulation.Models;

namespace dotGeoMigrata.Simulation.Engine;

/// <summary>
/// A built-in observer that outputs simulation progress to the console.
/// Useful for monitoring and debugging simulation execution.
/// </summary>
public sealed class ConsoleObserver : ISimulationObserver
{
    private readonly bool _verbose;

    /// <summary>
    /// Initializes a new instance of the ConsoleObserver.
    /// </summary>
    /// <param name="verbose">If true, outputs detailed information for each stage. Default is false.</param>
    public ConsoleObserver(bool verbose = false)
    {
        _verbose = verbose;
    }

    /// <inheritdoc />
    public void OnSimulationStart(SimulationContext context)
    {
        Console.WriteLine("=== Simulation Started ===");
        Console.WriteLine($"World: {context.World.DisplayName}");
        Console.WriteLine($"Cities: {context.World.Cities.Count}");
        Console.WriteLine($"Population Groups: {context.World.GroupDefinitions.Count}");
        Console.WriteLine($"Total Population: {context.World.Population:N0}");
        Console.WriteLine();
    }

    /// <inheritdoc />
    public void OnTickStart(SimulationContext context)
    {
        if (_verbose)
            Console.WriteLine($"--- Tick {context.CurrentTick} ---");
    }

    /// <inheritdoc />
    public void OnStageComplete(string stageName, SimulationContext context)
    {
        if (_verbose)
            Console.WriteLine($"  Stage '{stageName}' completed");
    }

    /// <inheritdoc />
    public void OnTickComplete(SimulationContext context)
    {
        Console.WriteLine($"Tick {context.CurrentTick}: " +
                          $"Population={context.World.Population:N0}, " +
                          $"Change={context.TotalPopulationChange:N0}, " +
                          $"Migrations={context.CurrentMigrationFlows.Count():N0}");

        if (!_verbose || !context.CurrentMigrationFlows.Any()) return;

        var topMigrations = context.CurrentMigrationFlows
            .OrderByDescending(f => f.MigrationCount)
            .Take(3);

        Console.WriteLine("  Top migrations:");
        foreach (var flow in topMigrations)
            Console.WriteLine($"    {flow.Group.DisplayName}: " +
                              $"{flow.OriginCity.DisplayName} → {flow.DestinationCity.DisplayName} " +
                              $"({flow.MigrationCount:F1} people)");
    }

    /// <inheritdoc />
    public void OnSimulationEnd(SimulationContext context, string reason)
    {
        Console.WriteLine();
        Console.WriteLine("=== Simulation Ended ===");
        Console.WriteLine($"Reason: {reason}");
        Console.WriteLine($"Total Ticks: {context.CurrentTick + 1}");
        Console.WriteLine($"Final Population: {context.World.Population:N0}");
        Console.WriteLine($"Stabilized: {context.IsStabilized}");
        Console.WriteLine();

        if (!_verbose) return;
        Console.WriteLine("Final City Populations:");
        foreach (var city in context.World.Cities.OrderByDescending(c => c.Population))
            Console.WriteLine($"  {city.DisplayName}: {city.Population:N0}");
    }

    /// <inheritdoc />
    public void OnError(SimulationContext context, Exception exception)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("=== ERROR ===");
        Console.WriteLine($"Tick: {context.CurrentTick}");
        Console.WriteLine($"Exception: {exception.GetType().Name}");
        Console.WriteLine($"Message: {exception.Message}");
        if (_verbose)
            Console.WriteLine($"Stack Trace: {exception.StackTrace}");

        Console.ResetColor();
        Console.WriteLine();
    }
}