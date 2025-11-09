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
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=== Simulation Started ===");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"World: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"{context.World.DisplayName}");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"Cities: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"{context.World.Cities.Count}");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"Population Groups: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"{context.World.GroupDefinitions.Count}");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"Total Population: ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"{context.World.Population:N0}");
        Console.ResetColor();
        Console.WriteLine();
    }

    /// <inheritdoc />
    public void OnTickStart(SimulationContext context)
    {
        if (!_verbose) return;
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"--- Tick {context.CurrentTick} ---");
        Console.ResetColor();
    }

    /// <inheritdoc />
    public void OnStageComplete(string stageName, SimulationContext context)
    {
        if (!_verbose) return;
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  Stage '{stageName}' completed");
        Console.ResetColor();
    }

    /// <inheritdoc />
    public void OnTickComplete(SimulationContext context)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"Tick {context.CurrentTick}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"Population=");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"{context.World.Population:N0}");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($", Change=");
        var changeColor = context.TotalPopulationChange >= 0 ? ConsoleColor.Green : ConsoleColor.Red;
        Console.ForegroundColor = changeColor;
        Console.Write($"{context.TotalPopulationChange:N0}");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($", Migrations=");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"{context.CurrentMigrationFlows.Count():N0}");
        Console.ResetColor();

        if (!_verbose || !context.CurrentMigrationFlows.Any()) return;

        var topMigrations = context.CurrentMigrationFlows
            .OrderByDescending(f => f.MigrationCount)
            .Take(3);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  Top migrations:");
        Console.ResetColor();
        foreach (var flow in topMigrations)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"    ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{flow.Group.DisplayName}: ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"{flow.OriginCity.DisplayName} → {flow.DestinationCity.DisplayName} ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"({flow.MigrationCount:F1} people)");
            Console.ResetColor();
        }
    }

    /// <inheritdoc />
    public void OnSimulationEnd(SimulationContext context, string reason)
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=== Simulation Ended ===");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"Reason: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"{reason}");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"Total Ticks: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"{context.CurrentTick + 1}");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"Final Population: ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"{context.World.Population:N0}");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"Stabilized: ");
        Console.ForegroundColor = context.IsStabilized ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"{context.IsStabilized}");
        Console.ResetColor();
        Console.WriteLine();

        if (!_verbose) return;
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Final City Populations:");
        Console.ResetColor();
        foreach (var city in context.World.Cities.OrderByDescending(c => c.Population))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"  {city.DisplayName}: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{city.Population:N0}");
            Console.ResetColor();
        }
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