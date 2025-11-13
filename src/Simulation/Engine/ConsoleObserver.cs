using dotGeoMigrata.Simulation.Interfaces;
using dotGeoMigrata.Simulation.Models;

namespace dotGeoMigrata.Simulation.Engine;

/// <summary>
/// A built-in observer that outputs simulation progress to the console.
/// Updated for person-based simulation monitoring.
/// </summary>
public sealed class ConsoleObserver : ISimulationObserver
{
    private readonly bool _colored;

    /// <summary>
    /// Initializes a new instance of the ConsoleObserver.
    /// </summary>
    /// <param name="colored">If true, outputs colored text. Default is false.</param>
    public ConsoleObserver(bool colored = false)
    {
        _colored = colored;
    }

    /// <inheritdoc />
    public void OnSimulationStart(SimulationContext context)
    {
        SetColor(ConsoleColor.Cyan);
        WriteLine("=== Simulation Started ===");
        ResetColor();

        SetColor(ConsoleColor.Yellow);
        Write("World: ");
        SetColor(ConsoleColor.White);
        WriteLine(context.World.DisplayName);

        SetColor(ConsoleColor.Yellow);
        Write("Cities: ");
        SetColor(ConsoleColor.White);
        WriteLine($"{context.World.Cities.Count}");

        SetColor(ConsoleColor.Yellow);
        Write("Total Population: ");
        SetColor(ConsoleColor.Green);
        WriteLine($"{context.World.Population:N0} persons");

        ResetColor();
        WriteLine();
    }

    /// <inheritdoc />
    public void OnTickStart(SimulationContext context)
    {
        SetColor(ConsoleColor.DarkGray);
        WriteLine($"--- Tick {context.CurrentTick} ---");
        ResetColor();
    }

    /// <inheritdoc />
    public void OnStageComplete(string stageName, SimulationContext context)
    {
        // Optionally log stage completion
    }

    /// <inheritdoc />
    public void OnTickComplete(SimulationContext context)
    {
        SetColor(ConsoleColor.Yellow);
        Write($"Tick {context.CurrentTick}: ");

        if (context.CurrentMigrationFlows.Any())
        {
            SetColor(ConsoleColor.White);
            WriteLine($"{context.CurrentMigrationFlows.Count():N0} persons migrated");
        }
        else
        {
            SetColor(ConsoleColor.DarkGray);
            WriteLine("No migrations");
        }

        // Show city populations
        foreach (var city in context.World.Cities.OrderByDescending(c => c.Population).Take(5))
        {
            SetColor(ConsoleColor.DarkCyan);
            Write($"  {city.DisplayName}: ");
            SetColor(ConsoleColor.Cyan);
            WriteLine($"{city.Population:N0} persons");
        }

        ResetColor();
        WriteLine();
    }

    /// <inheritdoc />
    public void OnSimulationEnd(SimulationContext context, string reason)
    {
        SetColor(ConsoleColor.Green);
        WriteLine($"=== Simulation Completed: {reason} ===");

        SetColor(ConsoleColor.Yellow);
        Write("Total Ticks: ");
        SetColor(ConsoleColor.White);
        WriteLine($"{context.CurrentTick}");

        SetColor(ConsoleColor.Yellow);
        Write("Final Population: ");
        SetColor(ConsoleColor.Green);
        WriteLine($"{context.World.Population:N0} persons");

        ResetColor();
        WriteLine();

        // Show final city populations
        WriteLine("Final City Populations:");
        foreach (var city in context.World.Cities.OrderByDescending(c => c.Population))
        {
            SetColor(ConsoleColor.Cyan);
            Write($"  {city.DisplayName}: ");
            SetColor(ConsoleColor.White);
            WriteLine($"{city.Population:N0} persons");
        }

        ResetColor();
    }

    /// <inheritdoc />
    public void OnError(SimulationContext context, Exception exception)
    {
        SetColor(ConsoleColor.Red);
        WriteLine($"!!! Error at Tick {context.CurrentTick}: {exception.Message}");
        ResetColor();
    }

    private void SetColor(ConsoleColor color)
    {
        if (_colored)
            Console.ForegroundColor = color;
    }

    private void ResetColor()
    {
        if (_colored)
            Console.ResetColor();
    }

    private static void Write(string message)
    {
        Console.Write(message);
    }


    private static void WriteLine(string message = "")
    {
        Console.WriteLine(message);
    }
}