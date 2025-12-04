using System.Text;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Engine;

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
    /// <param name="useUtf8Encoding">
    /// If true, sets console encoding to UTF-8 for international character support. Default is
    /// true.
    /// </param>
    public ConsoleObserver(bool colored = false, bool useUtf8Encoding = true)
    {
        _colored = colored;

        if (!useUtf8Encoding) return;
        try
        {
            // Set console encoding to UTF-8 to support international characters
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
        }
        catch
        {
            // Ignore encoding errors in environments where console encoding cannot be set
            // (e.g., some CI/CD environments or when output is redirected)
        }
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

        var migrationCount = context.CurrentMigrationFlows.Count();
        if (migrationCount > 0)
        {
            SetColor(ConsoleColor.White);
            WriteLine($"{migrationCount:N0} persons migrated");
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