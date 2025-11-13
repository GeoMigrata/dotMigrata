using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Logic.Calculators;
using dotGeoMigrata.Logic.Interfaces;
using dotGeoMigrata.Logic.Models;
using dotGeoMigrata.Simulation.Engine;
using dotGeoMigrata.Simulation.Interfaces;
using dotGeoMigrata.Simulation.Models;
using dotGeoMigrata.Simulation.Pipeline;

namespace dotGeoMigrata.Builder;

/// <summary>
/// Fluent builder for creating and configuring simulation engines.
/// Provides a convenient API for setting up complete simulations with minimal code.
/// </summary>
public sealed class SimulationBuilder
{
    private readonly List<ISimulationStage> _customStages = [];
    private readonly List<ISimulationObserver> _observers = [];
    private IAttractionCalculator? _attractionCalculator;
    private IMigrationCalculator? _migrationCalculator;
    private StandardModelConfig? _modelConfig;
    private SimulationConfig? _simulationConfig;
    private bool _useStandardPipeline = true;
    private World? _world;

    /// <summary>
    /// Sets the world to be simulated.
    /// </summary>
    /// <param name="world">The world containing cities, factors, and population groups.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SimulationBuilder WithWorld(World world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        return this;
    }

    /// <summary>
    /// Configures simulation execution parameters.
    /// </summary>
    /// <param name="config">Simulation configuration. If null, uses default configuration.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SimulationBuilder WithSimulationConfig(SimulationConfig? config = null)
    {
        _simulationConfig = config ?? SimulationConfig.Default;
        return this;
    }

    /// <summary>
    /// Configures the standard model parameters (attraction and migration calculations).
    /// </summary>
    /// <param name="config">Model configuration. If null, uses default configuration.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SimulationBuilder WithModelConfig(StandardModelConfig? config = null)
    {
        _modelConfig = config ?? StandardModelConfig.Default;
        return this;
    }

    /// <summary>
    /// Sets a custom attraction calculator.
    /// </summary>
    /// <param name="calculator">Custom attraction calculator implementation.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SimulationBuilder WithAttractionCalculator(IAttractionCalculator calculator)
    {
        _attractionCalculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        return this;
    }

    /// <summary>
    /// Sets a custom migration calculator.
    /// </summary>
    /// <param name="calculator">Custom migration calculator implementation.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SimulationBuilder WithMigrationCalculator(IMigrationCalculator calculator)
    {
        _migrationCalculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        return this;
    }

    /// <summary>
    /// Adds a custom simulation stage to the pipeline.
    /// When custom stages are added, the standard pipeline is not used unless explicitly enabled.
    /// </summary>
    /// <param name="stage">Custom simulation stage.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SimulationBuilder AddCustomStage(ISimulationStage stage)
    {
        _customStages.Add(stage ?? throw new ArgumentNullException(nameof(stage)));
        _useStandardPipeline = false;
        return this;
    }

    /// <summary>
    /// Configures a completely custom pipeline by providing all stages.
    /// This replaces any existing stages and disables the standard pipeline.
    /// </summary>
    /// <param name="stages">Collection of simulation stages to execute in order.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SimulationBuilder ConfigurePipeline(IEnumerable<ISimulationStage> stages)
    {
        ArgumentNullException.ThrowIfNull(stages);
        _customStages.Clear();
        _customStages.AddRange(stages);
        _useStandardPipeline = false;
        return this;
    }

    /// <summary>
    /// Configures a custom pipeline using a factory function.
    /// The factory receives the default calculators and can use them or replace them.
    /// </summary>
    /// <param name="pipelineFactory">Function that creates the pipeline stages.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SimulationBuilder ConfigurePipeline(
        Func<IAttractionCalculator, IMigrationCalculator, IEnumerable<ISimulationStage>> pipelineFactory)
    {
        ArgumentNullException.ThrowIfNull(pipelineFactory);

        var attractionCalc = _attractionCalculator ?? new StandardAttractionCalculator(_modelConfig);
        var migrationCalc = _migrationCalculator ?? new StandardMigrationCalculator(_modelConfig);

        var stages = pipelineFactory(attractionCalc, migrationCalc);
        return ConfigurePipeline(stages);
    }


    /// <summary>
    /// Enables the use of the standard simulation pipeline (attraction, migration decision, migration execution).
    /// This is the default behavior unless custom stages are added.
    /// </summary>
    /// <returns>The builder instance for method chaining.</returns>
    public SimulationBuilder UseStandardPipeline()
    {
        _useStandardPipeline = true;
        return this;
    }

    /// <summary>
    /// Adds an observer to monitor simulation events.
    /// </summary>
    /// <param name="observer">Simulation observer.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SimulationBuilder AddObserver(ISimulationObserver observer)
    {
        _observers.Add(observer ?? throw new ArgumentNullException(nameof(observer)));
        return this;
    }

    /// <summary>
    /// Adds a console observer to print simulation progress to the console.
    /// </summary>
    /// <param name="colored">Whether to use colored output (default: true).</param>
    /// <returns>The builder instance for method chaining.</returns>
    public SimulationBuilder AddConsoleObserver(bool colored = true)
    {
        _observers.Add(new ConsoleObserver(colored));
        return this;
    }

    /// <summary>
    /// Builds and returns a configured simulation engine.
    /// </summary>
    /// <returns>A configured simulation engine ready to run.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required components (world) are not set.</exception>
    public SimulationEngine Build()
    {
        if (_world == null)
            throw new InvalidOperationException("World must be set before building the simulation engine.");

        // Use provided calculators or create standard ones
        var attractionCalc = _attractionCalculator ?? new StandardAttractionCalculator(_modelConfig);
        var migrationCalc = _migrationCalculator ?? new StandardMigrationCalculator(_modelConfig);

        // Build stage pipeline
        var stages = _useStandardPipeline
            ? CreateStandardPipeline(attractionCalc, migrationCalc)
            : _customStages;

        if (stages.Count == 0)
            throw new InvalidOperationException(
                "No simulation stages configured. Use UseStandardPipeline() or add custom stages.");

        // Create engine
        var engine = new SimulationEngine(stages, _simulationConfig);

        // Add observers
        foreach (var observer in _observers)
            engine.AddObserver(observer);

        return engine;
    }

    /// <summary>
    /// Builds and immediately runs the simulation asynchronously.
    /// </summary>
    /// <returns>The final simulation context after completion.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required components are not set.</exception>
    public async Task<SimulationContext> BuildAndRunAsync()
    {
        if (_world == null)
            throw new InvalidOperationException("World must be set before running the simulation.");

        var engine = Build();
        return await engine.RunAsync(_world);
    }

    private static List<ISimulationStage> CreateStandardPipeline(
        IAttractionCalculator attractionCalculator,
        IMigrationCalculator migrationCalculator)
    {
        return
        [
            new MigrationDecisionStage(migrationCalculator, attractionCalculator),
            new MigrationExecutionStage()
        ];
    }
}