using System.Diagnostics;
using System.Runtime.CompilerServices;
using dotMigrata.Logic.Interfaces;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;

namespace dotMigrata.Simulation.Pipeline;

/// <summary>
/// Simulation stage that calculates migration decisions for all persons based on attraction differences.
/// Uses the migration calculator to determine which persons will migrate and to which cities.
/// </summary>
/// <remarks>
/// This stage uses parallel processing for optimal performance with large populations.
/// Performance characteristics: O(n) where n is the number of persons.
/// </remarks>
[DebuggerDisplay("Stage: {Name}, Ready: true")]
public sealed class MigrationDecisionStage : ISimulationStage, IDisposable
{
    /// <summary>
    /// Gets the constant name identifier for this stage.
    /// </summary>
    private const string StageName = "MigrationDecision";

    private readonly IAttractionCalculator _attractionCalculator;
    private readonly IMigrationCalculator _migrationCalculator;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the MigrationDecisionStage.
    /// </summary>
    /// <param name="migrationCalculator">The calculator to use for determining migration decisions.</param>
    /// <param name="attractionCalculator">The calculator to use for calculating attractions.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public MigrationDecisionStage(
        IMigrationCalculator migrationCalculator,
        IAttractionCalculator attractionCalculator)
    {
        _migrationCalculator = migrationCalculator ?? throw new ArgumentNullException(nameof(migrationCalculator));
        _attractionCalculator = attractionCalculator ?? throw new ArgumentNullException(nameof(attractionCalculator));
    }

    /// <summary>
    /// Releases resources used by this stage.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        if (_migrationCalculator is IDisposable disposableMigration)
            disposableMigration.Dispose();

        _disposed = true;
    }

    /// <inheritdoc />
    public string Name => StageName;

    /// <inheritdoc />
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ExecuteAsync(SimulationContext context)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Calculate all migration decisions using configured parallel processing settings
        var flows = _migrationCalculator
            .CalculateAllMigrationFlows(context.World, _attractionCalculator)
            .ToList();

        // Store flows in context for use by execution stage
        context.CurrentMigrationFlows = flows;
        context.SetData("MigrationFlows", flows);

        return Task.CompletedTask;
    }
}