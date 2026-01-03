using System.Text;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Display;

/// <summary>
/// A unified simulation reporter that displays configurable information during simulation execution.
/// Replaces the legacy <c>ConsoleObserver</c> and <c>DebugObserver</c> classes with a flexible,
/// option-based approach.
/// </summary>
/// <remarks>
/// Use <see cref="DisplayOption"/> flags to configure what information is displayed.
/// Use <see cref="DisplayPresets"/> for common configurations.
/// </remarks>
public sealed class SimulationReporter : ISimulationObserver
{
    private readonly DisplayOption _options;
    private readonly bool _colored;
    private readonly int _maxPersonSamples;
    private readonly int _maxCitiesToShow;
    private readonly int _maxMigrationRoutes;
    private readonly Dictionary<string, int> _initialPopulations = new();
    private int _totalMigrationsThisStep;

    /// <summary>
    /// Initializes a new instance of the <see cref="SimulationReporter"/> class.
    /// </summary>
    /// <param name="options">Display options specifying what information to show.</param>
    /// <param name="colored">If <see langword="true"/>, outputs colored text. Default is <see langword="true"/>.</param>
    /// <param name="maxPersonSamples">Maximum number of person samples to display. Default is 10.</param>
    /// <param name="maxCitiesToShow">Maximum number of cities to display in lists. Default is 5.</param>
    /// <param name="maxMigrationRoutes">Maximum number of migration routes to display. Default is 5.</param>
    /// <param name="useUtf8Encoding">
    /// If <see langword="true"/>, sets console encoding to UTF-8 for international character support.
    /// Default is <see langword="true"/>.
    /// </param>
    public SimulationReporter(
        DisplayOption options,
        bool colored = true,
        int maxPersonSamples = 10,
        int maxCitiesToShow = 5,
        int maxMigrationRoutes = 5,
        bool useUtf8Encoding = true)
    {
        _options = options;
        _colored = colored;
        _maxPersonSamples = maxPersonSamples;
        _maxCitiesToShow = maxCitiesToShow;
        _maxMigrationRoutes = maxMigrationRoutes;

        if (!useUtf8Encoding) return;
        try
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
        }
        catch
        {
            // Ignore encoding errors in environments where console encoding cannot be set
        }
    }

    /// <inheritdoc />
    public void OnSimulationStart(SimulationContext context)
    {
        // Store initial populations for tracking changes
        _initialPopulations.Clear();
        foreach (var city in context.World.Cities)
            _initialPopulations[city.DisplayName] = city.Population;

        if (ShouldDisplay(DisplayOption.WorldInfo))
            ShowWorldInfo(context);

        if (ShouldDisplay(DisplayOption.FactorDefinitions))
            ShowFactorDefinitions(context);

        if (ShouldDisplay(DisplayOption.CityDetails))
            ShowCityDetails(context);

        if (ShouldDisplay(DisplayOption.PopulationByTags))
            ShowPopulationByTags(context);

        if (!HasAnySimulationStartOption()) return;

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

        if (!ShouldDisplay(DisplayOption.StepHeader)) return;

        SetColor(ConsoleColor.Yellow);
        WriteLine("┌─────────────────────────────────────────────────────────────────┐");
        WriteLine($"│  STEP {context.CurrentStep,-3}                                                      │");
        WriteLine("└─────────────────────────────────────────────────────────────────┘");
        ResetColor();
    }

    /// <inheritdoc />
    public void OnStageComplete(string stageName, SimulationContext context)
    {
        if (!ShouldDisplay(DisplayOption.StageProgress)) return;

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

            if (ShouldDisplay(DisplayOption.MigrationFlows) && flowCount > 0)
            {
                WriteLine();
                ShowMigrationFlows(context);
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
        if (ShouldDisplay(DisplayOption.StepSummary))
            ShowStepSummary(context);

        if (ShouldDisplay(DisplayOption.PopulationChanges))
            ShowPopulationChanges(context);

        if (ShouldDisplay(DisplayOption.TopCities))
            ShowTopCities(context);

        if (HasAnyStepCompleteOption())
            WriteLine();
    }

    /// <inheritdoc />
    public void OnSimulationEnd(SimulationContext context, string reason)
    {
        if (ShouldDisplay(DisplayOption.CompletionInfo))
            ShowCompletionInfo(context, reason);

        if (ShouldDisplay(DisplayOption.FinalDistribution))
            ShowFinalDistribution(context);

        if (ShouldDisplay(DisplayOption.MigrationStats))
            ShowMigrationStats(context);

        if (ShouldDisplay(DisplayOption.PerformanceMetrics))
            ShowPerformanceMetrics(context);

        if (!HasAnySimulationEndOption()) return;

        SetColor(ConsoleColor.Green);
        WriteLine("═══════════════════════════════════════════════════════════════════");
        ResetColor();
    }

    /// <inheritdoc />
    public void OnError(SimulationContext context, Exception exception)
    {
        SetColor(ConsoleColor.Red);
        WriteLine("╔════════════════════════════════════════════════════════════════╗");
        WriteLine("║                           ERROR                                ║");
        WriteLine("╚════════════════════════════════════════════════════════════════╝");
        WriteLine();
        WriteLine($"Step: {context.CurrentStep}");
        WriteLine($"Error: {exception.Message}");

        if (ShouldDisplay(DisplayOption.PerformanceMetrics))
        {
            WriteLine();
            WriteLine("Stack Trace:");
            WriteLine(exception.StackTrace ?? "(no stack trace)");
        }

        ResetColor();
    }

    #region Display Methods

    private void ShowWorldInfo(SimulationContext context)
    {
        SetColor(ConsoleColor.Cyan);
        WriteLine("=== Simulation Started ===");
        ResetColor();

        WriteLabel("World: ");
        WriteValue(context.World.DisplayName);
        WriteLine();

        WriteLabel("Cities: ");
        WriteValue($"{context.World.Cities.Count}");
        WriteLine();

        WriteLabel("Total Population: ");
        SetColor(ConsoleColor.Green);
        Write($"{context.World.Population:N0} persons");
        ResetColor();
        WriteLine();
        WriteLine();
    }

    private void ShowFactorDefinitions(SimulationContext context)
    {
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
            Write($" ({factor.Type})");
            WriteLine();
        }

        WriteLine();
    }

    private void ShowCityDetails(SimulationContext context)
    {
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
    }

    private void ShowPopulationByTags(SimulationContext context)
    {
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
    }

    private void ShowMigrationFlows(SimulationContext context)
    {
        var flows = context.CurrentMigrationFlows.ToList();
        if (flows.Count == 0) return;

        SetColor(ConsoleColor.DarkGray);
        WriteLine("    ┌──────────────────────────────────────────────────────────────┐");
        WriteLine("    │  Migration Details:                                          │");
        WriteLine("    └──────────────────────────────────────────────────────────────┘");

        var flowGroups = flows
            .GroupBy(f => (f.OriginCity.DisplayName, f.DestinationCity.DisplayName))
            .OrderByDescending(g => g.Count())
            .Take(_maxMigrationRoutes)
            .ToList();

        foreach (var group in flowGroups)
        {
            SetColor(ConsoleColor.DarkCyan);
            Write($"      {group.Key.Item1}");
            SetColor(ConsoleColor.DarkGray);
            Write(" → ");
            SetColor(ConsoleColor.Cyan);
            Write($"{group.Key.Item2}");
            SetColor(ConsoleColor.DarkGray);
            WriteLine($": {group.Count():N0} persons");

            if (!ShouldDisplay(DisplayOption.PersonSamples)) continue;

            var samplePersons = group.Take(_maxPersonSamples).ToList();
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

            if (group.Count() <= _maxPersonSamples) continue;
            SetColor(ConsoleColor.DarkGray);
            WriteLine($"        ... and {group.Count() - _maxPersonSamples:N0} more");
        }

        if (flowGroups.Count < _maxMigrationRoutes || flowGroups.Count >=
            flows.GroupBy(f => (f.OriginCity.DisplayName, f.DestinationCity.DisplayName)).Count()) return;
        SetColor(ConsoleColor.DarkGray);
        WriteLine($"      ... and more migration routes");
    }

    private void ShowStepSummary(SimulationContext context)
    {
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
    }

    private void ShowPopulationChanges(SimulationContext context)
    {
        WriteLabel("  City Populations: ");
        WriteLine();

        foreach (var city in context.World.Cities.OrderByDescending(c => c.Population))
        {
            var initial = _initialPopulations.GetValueOrDefault(city.DisplayName, city.Population);
            var change = city.Population - initial;
            var changeStr = change > 0 ? $"+{change}" : change.ToString();
            var changeColor = change > 0 ? ConsoleColor.Green :
                change < 0 ? ConsoleColor.Red : ConsoleColor.DarkGray;

            SetColor(ConsoleColor.Cyan);
            Write($"    {city.DisplayName}: ");
            SetColor(ConsoleColor.White);
            Write($"{city.Population:N0}");
            SetColor(changeColor);
            WriteLine($" ({changeStr})");
        }

        ResetColor();
    }

    private void ShowTopCities(SimulationContext context)
    {
        var topCities = context.World.Cities.OrderByDescending(c => c.Population).Take(_maxCitiesToShow);

        foreach (var city in topCities)
        {
            SetColor(ConsoleColor.DarkCyan);
            Write($"  {city.DisplayName}: ");
            SetColor(ConsoleColor.Cyan);
            WriteLine($"{city.Population:N0} persons");
        }

        ResetColor();
    }

    private void ShowCompletionInfo(SimulationContext context, string reason)
    {
        WriteLine();
        SetColor(ConsoleColor.Green);
        WriteLine($"=== Simulation Completed: {reason} ===");
        ResetColor();

        WriteLabel("Total Steps: ");
        WriteValue($"{context.CurrentStep}");
        WriteLine();

        WriteLabel("Final Population: ");
        SetColor(ConsoleColor.Green);
        Write($"{context.World.Population:N0} persons");
        ResetColor();
        WriteLine();

        WriteLabel("Stabilized: ");
        WriteValue(context.IsStabilized ? "Yes" : "No");
        WriteLine();
        WriteLine();
    }

    private void ShowFinalDistribution(SimulationContext context)
    {
        WriteLine("Final City Populations:");
        foreach (var city in context.World.Cities.OrderByDescending(c => c.Population))
        {
            SetColor(ConsoleColor.Cyan);
            Write($"  {city.DisplayName}: ");
            SetColor(ConsoleColor.White);
            WriteLine($"{city.Population:N0} persons");
        }

        ResetColor();
        WriteLine();
    }

    private void ShowMigrationStats(SimulationContext context)
    {
        var totalChange = (from city in context.World.Cities
            let initial = _initialPopulations.GetValueOrDefault(city.DisplayName, city.Population)
            select Math.Abs(city.Population - initial)).Sum();

        WriteLabel("Total Migrations: ");
        WriteValue($"~{totalChange / 2:N0}");
        WriteLine();
        WriteLine();

        WriteLine("Population Changes by City:");
        foreach (var city in context.World.Cities.OrderByDescending(c => c.Population))
        {
            var initial = _initialPopulations.GetValueOrDefault(city.DisplayName, city.Population);
            var final = city.Population;
            var change = final - initial;
            var changeStr = change >= 0 ? $"+{change:N0}" : $"{change:N0}";
            var changeColor = change > 0 ? ConsoleColor.Green :
                change < 0 ? ConsoleColor.Red : ConsoleColor.DarkGray;

            SetColor(ConsoleColor.Cyan);
            Write($"  {city.DisplayName,-20} ");
            SetColor(ConsoleColor.White);
            Write($"{final,8:N0}");
            SetColor(ConsoleColor.DarkGray);
            Write($" (was {initial,8:N0}) ");
            SetColor(changeColor);
            WriteLine($" {changeStr,8}");
        }

        ResetColor();
        WriteLine();
    }

    private void ShowPerformanceMetrics(SimulationContext context)
    {
        WriteLabel("Performance Metrics: ");
        WriteLine();

        SetColor(ConsoleColor.DarkGray);
        WriteLine($"  Total Time: {context.Performance.TotalElapsed.TotalSeconds:F2}s");
        WriteLine($"  Average Step: {context.Performance.AverageStepDuration.TotalMilliseconds:F2}ms");
        WriteLine($"  Steps/Second: {context.Performance.StepsPerSecond:F2}");
        WriteLine($"  Memory: {PerformanceMetrics.CurrentMemoryBytes / 1024.0 / 1024.0:F2} MB");
        ResetColor();
        WriteLine();
    }

    #endregion

    #region Helper Methods

    private bool ShouldDisplay(DisplayOption option) => (_options & option) == option;

    private bool HasAnySimulationStartOption() =>
        ShouldDisplay(DisplayOption.WorldInfo) ||
        ShouldDisplay(DisplayOption.FactorDefinitions) ||
        ShouldDisplay(DisplayOption.CityDetails) ||
        ShouldDisplay(DisplayOption.PopulationByTags);

    private bool HasAnyStepCompleteOption() =>
        ShouldDisplay(DisplayOption.StepSummary) ||
        ShouldDisplay(DisplayOption.PopulationChanges) ||
        ShouldDisplay(DisplayOption.TopCities);

    private bool HasAnySimulationEndOption() =>
        ShouldDisplay(DisplayOption.CompletionInfo) ||
        ShouldDisplay(DisplayOption.FinalDistribution) ||
        ShouldDisplay(DisplayOption.MigrationStats) ||
        ShouldDisplay(DisplayOption.PerformanceMetrics);

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

    #endregion
}