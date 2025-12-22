using System.Text;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Engine;

/// <summary>
/// A debug observer that provides detailed simulation output for development and troubleshooting.
/// Shows comprehensive information about migration decisions, attraction scores, and population dynamics.
/// </summary>
public sealed class DebugObserver : ISimulationObserver
{
    private readonly bool _colored;
    private readonly Dictionary<string, int> _initialPopulations = new();
    private readonly int _maxPersonsToShow;
    private readonly bool _showPersonDetails;
    private int _totalMigrationsThisStep;

    /// <summary>
    /// Initializes a new instance of the DebugObserver.
    /// </summary>
    /// <param name="colored">If true, outputs colored text. Default is true.</param>
    /// <param name="showPersonDetails">If true, shows individual person details during migration. Default is true.</param>
    /// <param name="maxPersonsToShow">Maximum number of persons to show details for per step. Default is 10.</param>
    public DebugObserver(bool colored = true, bool showPersonDetails = true, int maxPersonsToShow = 10)
    {
        _colored = colored;
        _showPersonDetails = showPersonDetails;
        _maxPersonsToShow = maxPersonsToShow;

        try
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
        }
        catch
        {
            // Ignore encoding errors
        }
    }

    /// <inheritdoc />
    public void OnSimulationStart(SimulationContext context)
    {
        SetColor(ConsoleColor.Magenta);
        WriteLine("╔════════════════════════════════════════════════════════════════╗");
        WriteLine("║                    DEBUG MODE - SIMULATION START               ║");
        WriteLine("╚════════════════════════════════════════════════════════════════╝");
        ResetColor();
        WriteLine();

        // Store initial populations for tracking
        _initialPopulations.Clear();
        foreach (var city in context.World.Cities) _initialPopulations[city.DisplayName] = city.Population;

        // World details
        WriteLabel("World Name: ");
        WriteValue(context.World.DisplayName);
        WriteLine();

        WriteLabel("Factor Definitions: ");
        WriteValue($"{context.World.FactorDefinitions.Count}");
        WriteLine();

        foreach (var factor in context.World.FactorDefinitions)
        {
            SetColor(ConsoleColor.DarkGray);
            Write("  • ");
            SetColor(ConsoleColor.White);
            Write(factor.DisplayName);
            SetColor(ConsoleColor.DarkGray);
            Write($" ({factor.Type}, Range: {factor.MinValue}-{factor.MaxValue})");
            WriteLine();
        }

        WriteLine();

        // City details
        WriteLabel("Cities: ");
        WriteValue($"{context.World.Cities.Count}");
        WriteLine();

        foreach (var city in context.World.Cities)
        {
            SetColor(ConsoleColor.Cyan);
            WriteLine($"  📍 {city.DisplayName}");
            SetColor(ConsoleColor.DarkGray);
            WriteLine($"     Location: ({city.Location.Latitude:F4}, {city.Location.Longitude:F4})");
            WriteLine($"     Population: {city.Population:N0}");
            WriteLine($"     Capacity: {city.Capacity:N0}");
            Write("     Factors: ");
            var factorIntensities = city.FactorIntensities.Select(fi =>
                $"{fi.Definition.DisplayName}={fi.Value}");
            WriteLine(string.Join(", ", factorIntensities));
        }

        WriteLine();

        // Population statistics
        WriteLabel("Total Population: ");
        WriteValue($"{context.World.Population:N0}");
        WriteLine();

        // Show population distribution by tags (only for StandardPerson)
        var allPersons = context.World.Cities.SelectMany(c => c.Persons).ToList();
        var tagGroups = allPersons
            .SelectMany(p => p.Tags.DefaultIfEmpty("unknown"))
            .GroupBy(t => t)
            .OrderByDescending(g => g.Count())
            .Take(10);

        WriteLabel("Population by Tags: ");
        WriteLine();
        foreach (var group in tagGroups)
        {
            SetColor(ConsoleColor.DarkGray);
            WriteLine($"  • {group.Key}: {group.Count():N0}");
        }

        ResetColor();
        WriteLine();

        SetColor(ConsoleColor.Green);
        WriteLine("═══════════════════════════════════════════════════════════════════");
        ResetColor();
        WriteLine();
    }

    /// <inheritdoc />
    public void OnStepStart(SimulationContext context)
    {
        _totalMigrationsThisStep = 0;
        SetColor(ConsoleColor.Yellow);
        WriteLine("┌─────────────────────────────────────────────────────────────────┐");
        WriteLine($"│  STEP {context.CurrentStep,-3}                                                      │");
        WriteLine("└─────────────────────────────────────────────────────────────────┘");
        ResetColor();
    }

    /// <inheritdoc />
    public void OnStageComplete(string stageName, SimulationContext context)
    {
        SetColor(ConsoleColor.DarkMagenta);
        Write("  ► Stage Complete: ");
        SetColor(ConsoleColor.White);
        Write(stageName);

        if (stageName.Contains("Decision", StringComparison.OrdinalIgnoreCase))
        {
            var flowCount = context.CurrentMigrationFlows.Count();
            _totalMigrationsThisStep = flowCount;
            SetColor(ConsoleColor.DarkGray);
            Write($" ({flowCount:N0} migration decisions)");

            if (_showPersonDetails && flowCount > 0)
            {
                WriteLine();
                ShowMigrationDetails(context);
            }
        }
        else if (stageName.Contains("Execution", StringComparison.OrdinalIgnoreCase))
        {
            SetColor(ConsoleColor.DarkGray);
            Write($" ({_totalMigrationsThisStep:N0} migrations executed)");
        }

        ResetColor();
        WriteLine();
    }

    /// <inheritdoc />
    public void OnStepComplete(SimulationContext context)
    {
        WriteLine();
        WriteLabel("  Step Summary: ");

        if (_totalMigrationsThisStep > 0)
        {
            SetColor(ConsoleColor.Green);
            Write($"{_totalMigrationsThisStep:N0} migrations");
        }
        else
        {
            SetColor(ConsoleColor.DarkGray);
            Write("No migrations");
        }

        ResetColor();
        WriteLine();

        // Show current city populations with change from initial
        WriteLabel("  City Populations: ");
        WriteLine();
        foreach (var city in context.World.Cities.OrderByDescending(c => c.Population))
        {
            var initial = _initialPopulations.GetValueOrDefault(city.DisplayName, city.Population);
            var change = city.Population - initial;
            var changeStr = change > 0 ? $"+{change}" : change.ToString();
            var changeColor = change > 0 ? ConsoleColor.Green : change < 0 ? ConsoleColor.Red : ConsoleColor.DarkGray;

            SetColor(ConsoleColor.Cyan);
            Write($"    {city.DisplayName}: ");
            SetColor(ConsoleColor.White);
            Write($"{city.Population:N0}");
            SetColor(changeColor);
            WriteLine($" ({changeStr})");
        }

        ResetColor();
        WriteLine();
    }

    /// <inheritdoc />
    public void OnSimulationEnd(SimulationContext context, string reason)
    {
        WriteLine();
        SetColor(ConsoleColor.Magenta);
        WriteLine("╔════════════════════════════════════════════════════════════════╗");
        WriteLine("║                    DEBUG MODE - SIMULATION END                 ║");
        WriteLine("╚════════════════════════════════════════════════════════════════╝");
        ResetColor();
        WriteLine();

        WriteLabel("Termination Reason: ");
        WriteValue(reason);
        WriteLine();

        WriteLabel("Total Steps: ");
        WriteValue($"{context.CurrentStep}");
        WriteLine();

        WriteLabel("Stabilized: ");
        WriteValue(context.IsStabilized ? "Yes" : "No");
        WriteLine();

        WriteLabel("Final Population: ");
        WriteValue($"{context.World.Population:N0}");
        WriteLine();
        WriteLine();

        // Final population analysis
        SetColor(ConsoleColor.Yellow);
        WriteLine("Final Population Distribution:");
        ResetColor();

        var totalChange = 0;
        foreach (var city in context.World.Cities.OrderByDescending(c => c.Population))
        {
            var initial = _initialPopulations.GetValueOrDefault(city.DisplayName, city.Population);
            var final = city.Population;
            var change = final - initial;
            totalChange += Math.Abs(change);
            var changeStr = change >= 0 ? $"+{change:N0}" : $"{change:N0}";
            var changeColor = change > 0 ? ConsoleColor.Green : change < 0 ? ConsoleColor.Red : ConsoleColor.DarkGray;

            SetColor(ConsoleColor.Cyan);
            Write($"  {city.DisplayName,-20} ");
            SetColor(ConsoleColor.White);
            Write($"{final,8:N0}");
            SetColor(ConsoleColor.DarkGray);
            Write($" (was {initial,8:N0}) ");
            SetColor(changeColor);
            WriteLine($" {changeStr,8}");
        }

        WriteLine();
        WriteLabel("Total Migrations: ");
        WriteValue($"~{totalChange / 2:N0}");
        WriteLine();

        SetColor(ConsoleColor.Green);
        WriteLine("═══════════════════════════════════════════════════════════════════");
        ResetColor();
    }

    /// <inheritdoc />
    public void OnError(SimulationContext context, Exception exception)
    {
        SetColor(ConsoleColor.Red);
        WriteLine("╔════════════════════════════════════════════════════════════════╗");
        WriteLine("║                    DEBUG MODE - ERROR                          ║");
        WriteLine("╚════════════════════════════════════════════════════════════════╝");
        WriteLine();
        WriteLine($"Step: {context.CurrentStep}");
        WriteLine($"Error: {exception.Message}");
        WriteLine();
        WriteLine("Stack Trace:");
        WriteLine(exception.StackTrace ?? "(no stack trace)");
        ResetColor();
    }

    private void ShowMigrationDetails(SimulationContext context)
    {
        var flows = context.CurrentMigrationFlows.ToList();
        if (flows.Count == 0) return;

        SetColor(ConsoleColor.DarkGray);
        WriteLine("    ┌──────────────────────────────────────────────────────────────┐");
        WriteLine("    │  Migration Details:                                          │");
        WriteLine("    └──────────────────────────────────────────────────────────────┘");

        // Group by origin -> destination
        var flowGroups = flows
            .GroupBy(f => (f.OriginCity.DisplayName, f.DestinationCity.DisplayName))
            .OrderByDescending(g => g.Count())
            .ToList();

        foreach (var group in flowGroups.Take(5))
        {
            SetColor(ConsoleColor.DarkCyan);
            Write($"      {group.Key.Item1}");
            SetColor(ConsoleColor.DarkGray);
            Write(" → ");
            SetColor(ConsoleColor.Cyan);
            Write($"{group.Key.Item2}");
            SetColor(ConsoleColor.DarkGray);
            WriteLine($": {group.Count():N0} persons");

            // Show sample persons
            if (!_showPersonDetails) continue;
            var samplePersons = group.Take(_maxPersonsToShow).ToList();
            foreach (var flow in samplePersons)
            {
                var tags = flow.Person.Tags.Any()
                    ? string.Join(", ", flow.Person.Tags)
                    : "no tags";
                SetColor(ConsoleColor.DarkGray);
                Write("        • ");
                SetColor(ConsoleColor.Gray);
                Write($"[{tags}] ");
                SetColor(ConsoleColor.DarkGray);
                Write($"Willingness: {flow.Person.MovingWillingness.Value:F2}, ");
                Write($"Prob: {flow.MigrationProbability}");
                WriteLine();
            }

            if (group.Count() <= _maxPersonsToShow) continue;
            SetColor(ConsoleColor.DarkGray);
            WriteLine($"        ... and {group.Count() - _maxPersonsToShow:N0} more");
        }

        if (flowGroups.Count <= 5) return;
        SetColor(ConsoleColor.DarkGray);
        WriteLine($"      ... and {flowGroups.Count - 5} more migration routes");
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

    private void WriteLabel(string label)
    {
        SetColor(ConsoleColor.Yellow);
        Write(label);
        ResetColor();
    }

    private void WriteValue(string value)
    {
        SetColor(ConsoleColor.White);
        Write(value);
        ResetColor();
    }

    private static void Write(string message) => Console.Write(message);

    private static void WriteLine(string message = "") => Console.WriteLine(message);
}