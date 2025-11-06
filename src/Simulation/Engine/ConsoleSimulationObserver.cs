using dotGeoMigrata.Logic.Migration;
using dotGeoMigrata.Simulation.State;

namespace dotGeoMigrata.Simulation.Engine;

/// <summary>
/// A simple console observer that logs simulation progress to the console.
/// </summary>
public class ConsoleSimulationObserver : ISimulationObserver
{
    private readonly bool _verbose;

    public ConsoleSimulationObserver(bool verbose = false)
    {
        _verbose = verbose;
    }

    public void OnSimulationStarted(SimulationState state)
    {
        Console.WriteLine("=== Simulation Started ===");
        Console.WriteLine($"Initial Step: {state.CurrentStep}");
        Console.WriteLine();
    }

    public void OnStepCompleted(SimulationState state, IReadOnlyList<MigrationFlow> migrationFlows)
    {
        Console.WriteLine($"Step {state.CurrentStep} completed:");
        Console.WriteLine($"  Migrations this step: {state.LastStepMigrations}");
        Console.WriteLine($"  Total migrations: {state.TotalMigrations}");

        if (_verbose && migrationFlows.Count > 0)
        {
            Console.WriteLine("  Migration details:");
            foreach (var flow in migrationFlows)
                Console.WriteLine($"    {flow.SourceCity.DisplayName} -> {flow.DestinationCity.DisplayName}: " +
                                  $"{flow.MigrantCount} people ({flow.PopulationGroupDefinition.DisplayName})");
        }

        Console.WriteLine();
    }

    public void OnSimulationCompleted(SimulationState state)
    {
        Console.WriteLine("=== Simulation Completed ===");
        Console.WriteLine($"Final Step: {state.CurrentStep}");
        Console.WriteLine($"Total Migrations: {state.TotalMigrations}");
        Console.WriteLine($"Stabilized: {state.IsStabilized}");
        Console.WriteLine();
    }
}