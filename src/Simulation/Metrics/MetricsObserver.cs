using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Metrics;

/// <summary>
/// Observer that collects simulation metrics at each step.
/// Integrates with the MetricsCollector for comprehensive statistics tracking.
/// </summary>
public sealed class MetricsObserver : ISimulationObserver
{
    /// <summary>
    /// Gets the metrics collector containing all collected data.
    /// </summary>
    public MetricsCollector Collector { get; } = new();

    /// <inheritdoc />
    public void OnSimulationStart(SimulationContext context)
    {
        Collector.Clear();
        // Collect initial state (step -1 or 0 before any changes)
        Collector.Collect(context.World, -1);
    }

    /// <inheritdoc />
    public void OnStepStart(SimulationContext context)
    {
        // Nothing to do at step start
    }

    /// <inheritdoc />
    public void OnStageComplete(string stageName, SimulationContext context)
    {
        // Nothing to do at stage complete
    }

    /// <inheritdoc />
    public void OnStepComplete(SimulationContext context)
    {
        // Calculate migration counts per city
        var incomingMigrations = new Dictionary<string, int>();
        var outgoingMigrations = new Dictionary<string, int>();

        foreach (var flow in context.CurrentMigrationFlows)
        {
            // Count incoming migrations
            var destName = flow.DestinationCity.DisplayName;
            incomingMigrations[destName] = incomingMigrations.GetValueOrDefault(destName) + 1;

            // Count outgoing migrations
            var originName = flow.OriginCity.DisplayName;
            outgoingMigrations[originName] = outgoingMigrations.GetValueOrDefault(originName) + 1;
        }

        Collector.Collect(context.World, context.CurrentStep, incomingMigrations, outgoingMigrations);
    }

    /// <inheritdoc />
    public void OnSimulationEnd(SimulationContext context, string reason)
    {
        // Final metrics are already collected in OnStepComplete
    }

    /// <inheritdoc />
    public void OnError(SimulationContext context, Exception exception)
    {
        // Nothing to do on error
    }
}