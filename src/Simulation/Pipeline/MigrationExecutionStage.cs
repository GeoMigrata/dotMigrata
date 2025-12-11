using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using dotMigrata.Logic.Models;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Pipeline;

/// <summary>
/// Simulation stage that executes migration flows by moving persons between cities.
/// Processes the migration flows calculated by MigrationDecisionStage.
/// </summary>
/// <remarks>
/// This stage is optimized for thread-safe execution with ConcurrentDictionary for tracking city changes.
/// Performance characteristics: O(n) where n is the number of migration flows.
/// </remarks>
[DebuggerDisplay("Stage: {Name}, Ready: true")]
public sealed class MigrationExecutionStage : ISimulationStage
{
    /// <inheritdoc />
    public string Name => StageName;

    /// <summary>
    /// Gets the constant name identifier for this stage.
    /// </summary>
    private const string StageName = "MigrationExecution";

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ExecuteAsync(SimulationContext context)
    {
        var flows = context.GetData<List<MigrationFlow>>("MigrationFlows");

        if (flows is not { Count: > 0 })
        {
            ResetPopulationChanges(context);
            return Task.CompletedTask;
        }

        // Track population changes per city (thread-safe for potential future parallel execution)
        var cityChanges = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);

        // Execute all migrations
        foreach (var flow in flows)
            ExecuteMigration(flow, cityChanges);

        // Calculate and update statistics
        UpdatePopulationStatistics(context, cityChanges);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Executes a single migration flow by moving a person between cities.
    /// </summary>
    /// <param name="flow">The migration flow to execute.</param>
    /// <param name="cityChanges">Dictionary tracking population changes per city.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ExecuteMigration(MigrationFlow flow, ConcurrentDictionary<string, int> cityChanges)
    {
        var person = flow.Person;
        var originCity = flow.OriginCity;
        var destinationCity = flow.DestinationCity;

        // Remove person from origin city
        if (originCity.RemovePerson(person))
            cityChanges.AddOrUpdate(originCity.DisplayName, 1, (_, count) => count + 1);

        // Add person to destination city
        destinationCity.AddPerson(person);
        cityChanges.AddOrUpdate(destinationCity.DisplayName, 1, (_, count) => count + 1);
    }

    /// <summary>
    /// Updates the simulation context with population change statistics.
    /// </summary>
    /// <param name="context">The simulation context to update.</param>
    /// <param name="cityChanges">Dictionary containing population changes per city.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void UpdatePopulationStatistics(SimulationContext context,
        ConcurrentDictionary<string, int> cityChanges)
    {
        var totalChange = cityChanges.Values.Sum();
        var maxCityChange = cityChanges.Values.Count != 0 ? cityChanges.Values.Max() : 0;

        context.TotalPopulationChange = totalChange;
        context.MaxCityPopulationChange = maxCityChange;
    }

    /// <summary>
    /// Resets population change tracking to zero.
    /// </summary>
    /// <param name="context">The simulation context to reset.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ResetPopulationChanges(SimulationContext context)
    {
        context.TotalPopulationChange = 0;
        context.MaxCityPopulationChange = 0;
    }
}