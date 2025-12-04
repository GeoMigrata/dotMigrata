using dotMigrata.Logic.Calculators;
using dotMigrata.Logic.Interfaces;
using dotMigrata.Logic.Models;
using dotMigrata.Simulation.Engine;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;
using dotMigrata.Simulation.Pipeline;

namespace dotMigrata.Simulation.Builders;

/// <summary>
/// Fluent builder for creating and configuring simulation engines.
/// Provides a streamlined API for setting up simulations with sensible defaults.
/// </summary>
public sealed class SimulationBuilder
{
    private readonly List<ISimulationStage> _stages = [];
    private readonly List<ISimulationObserver> _observers = [];
    private SimulationConfig _simulationConfig = SimulationConfig.Default;
    private StandardModelConfig _modelConfig = StandardModelConfig.Default;
    private IAttractionCalculator? _attractionCalculator;
    private IMigrationCalculator? _migrationCalculator;
    private int? _randomSeed;
    
    /// <summary>
    /// Creates a new simulation builder instance.
    /// </summary>
    /// <returns>A new SimulationBuilder.</returns>
    public static SimulationBuilder Create() => new();

    /// <summary>
    /// Uses default stages for migration simulation (decision + execution).
    /// This is the most common setup for migration simulations.
    /// </summary>
    /// <returns>This builder for method chaining.</returns>
    public SimulationBuilder UseDefaultMigrationStages()
    {
        _stages.Clear();
        // Stages will be created in Build() with the configured calculators
        return this;
    }

    /// <summary>
    /// Adds a custom simulation stage.
    /// </summary>
    /// <param name="stage">The stage to add.</param>
    /// <returns>This builder for method chaining.</returns>
    public SimulationBuilder AddStage(ISimulationStage stage)
    {
        ArgumentNullException.ThrowIfNull(stage);
        _stages.Add(stage);
        return this;
    }

    /// <summary>
    /// Adds an observer for monitoring simulation progress.
    /// </summary>
    /// <param name="observer">The observer to add.</param>
    /// <returns>This builder for method chaining.</returns>
    public SimulationBuilder AddObserver(ISimulationObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        _observers.Add(observer);
        return this;
    }

    /// <summary>
    /// Adds a console observer for debugging output.
    /// </summary>
    /// <param name="colored">Whether to use colored output.</param>
    /// <returns>This builder for method chaining.</returns>
    public SimulationBuilder WithConsoleOutput(bool colored = true)
    {
        _observers.Add(new ConsoleObserver(colored));
        return this;
    }

    /// <summary>
    /// Configures the simulation parameters.
    /// </summary>
    /// <param name="config">The simulation configuration.</param>
    /// <returns>This builder for method chaining.</returns>
    public SimulationBuilder WithSimulationConfig(SimulationConfig config)
    {
        _simulationConfig = config ?? throw new ArgumentNullException(nameof(config));
        return this;
    }

    /// <summary>
    /// Configures the simulation parameters using a builder action.
    /// </summary>
    /// <param name="configure">Action to configure the simulation.</param>
    /// <returns>This builder for method chaining.</returns>
    public SimulationBuilder ConfigureSimulation(Action<SimulationConfigBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var builder = new SimulationConfigBuilder();
        configure(builder);
        _simulationConfig = builder.Build();
        return this;
    }

    /// <summary>
    /// Configures the mathematical model parameters.
    /// </summary>
    /// <param name="config">The model configuration.</param>
    /// <returns>This builder for method chaining.</returns>
    public SimulationBuilder WithModelConfig(StandardModelConfig config)
    {
        _modelConfig = config ?? throw new ArgumentNullException(nameof(config));
        return this;
    }

    /// <summary>
    /// Configures the mathematical model parameters using a builder action.
    /// </summary>
    /// <param name="configure">Action to configure the model.</param>
    /// <returns>This builder for method chaining.</returns>
    public SimulationBuilder ConfigureModel(Action<ModelConfigBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var builder = new ModelConfigBuilder();
        configure(builder);
        _modelConfig = builder.Build();
        return this;
    }

    /// <summary>
    /// Sets a custom attraction calculator.
    /// </summary>
    /// <param name="calculator">The attraction calculator to use.</param>
    /// <returns>This builder for method chaining.</returns>
    public SimulationBuilder WithAttractionCalculator(IAttractionCalculator calculator)
    {
        _attractionCalculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        return this;
    }

    /// <summary>
    /// Sets a custom migration calculator.
    /// </summary>
    /// <param name="calculator">The migration calculator to use.</param>
    /// <returns>This builder for method chaining.</returns>
    public SimulationBuilder WithMigrationCalculator(IMigrationCalculator calculator)
    {
        _migrationCalculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        return this;
    }

    /// <summary>
    /// Sets a random seed for reproducible simulations.
    /// </summary>
    /// <param name="seed">The random seed.</param>
    /// <returns>This builder for method chaining.</returns>
    public SimulationBuilder WithRandomSeed(int seed)
    {
        _randomSeed = seed;
        return this;
    }

    /// <summary>
    /// Builds the configured simulation engine.
    /// </summary>
    /// <returns>A configured SimulationEngine instance.</returns>
    public SimulationEngine Build()
    {
        // Create default calculators if not provided
        _attractionCalculator ??= new StandardAttractionCalculator(_modelConfig);
        _migrationCalculator ??= new StandardMigrationCalculator(_modelConfig, _randomSeed);

        // Create default stages if none provided
        var stages = _stages.Count > 0
            ? _stages
            : CreateDefaultStages();

        var engine = new SimulationEngine(stages, _simulationConfig);

        foreach (var observer in _observers)
        {
            engine.AddObserver(observer);
        }

        return engine;
    }

    private List<ISimulationStage> CreateDefaultStages()
    {
        return
        [
            new MigrationDecisionStage(_migrationCalculator!, _attractionCalculator!),
            new MigrationExecutionStage()
        ];
    }
}