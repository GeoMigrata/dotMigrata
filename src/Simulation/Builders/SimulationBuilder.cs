using dotMigrata.Logic.Calculators;
using dotMigrata.Logic.Interfaces;
using dotMigrata.Logic.Models;
using dotMigrata.Simulation.Engine;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;
using dotMigrata.Simulation.Pipeline;

namespace dotMigrata.Simulation.Builders;

/// <summary>
/// Provides a fluent API for creating and configuring simulation engines.
/// </summary>
/// <remarks>
/// Provides a streamlined API for setting up simulations with sensible defaults.
/// </remarks>
public sealed class SimulationBuilder
{
    private readonly List<ISimulationObserver> _observers = [];
    private readonly List<ISimulationStage> _stages = [];
    private IAttractionCalculator? _attractionCalculator;
    private IMigrationCalculator? _migrationCalculator;
    private IStabilityCriteria? _stabilityCriteria;
    private StandardModelConfig _modelConfig = StandardModelConfig.Default;
    private int? _randomSeed;
    private SimulationConfig _simulationConfig = SimulationConfig.Default;

    /// <summary>
    /// Creates a new <see cref="SimulationBuilder" /> instance.
    /// </summary>
    /// <returns>A new <see cref="SimulationBuilder" />.</returns>
    public static SimulationBuilder Create()
    {
        return new SimulationBuilder();
    }

    /// <summary>
    /// Uses default stages for migration simulation (decision and execution).
    /// </summary>
    /// <returns>This builder for method chaining.</returns>
    /// <remarks>
    /// This is the most common setup for migration simulations.
    /// </remarks>
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
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stage" /> is <see langword="null" />.
    /// </exception>
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
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="observer" /> is <see langword="null" />.
    /// </exception>
    public SimulationBuilder AddObserver(ISimulationObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        _observers.Add(observer);
        return this;
    }

    /// <summary>
    /// Adds a console observer for basic simulation output.
    /// </summary>
    /// <param name="colored">
    /// <see langword="true" /> to use colored output; otherwise, <see langword="false" />.
    /// </param>
    /// <returns>This builder for method chaining.</returns>
    public SimulationBuilder WithConsoleOutput(bool colored = true)
    {
        _observers.Add(new ConsoleObserver(colored));
        return this;
    }

    /// <summary>
    /// Adds a debug observer for comprehensive simulation debugging output.
    /// Shows detailed information about migration decisions, attraction scores, and population dynamics.
    /// </summary>
    /// <param name="colored">
    /// <see langword="true" /> to use colored output; otherwise, <see langword="false" />.
    /// </param>
    /// <param name="showPersonDetails">
    /// <see langword="true" /> to show individual person details during migration; otherwise, <see langword="false" />.
    /// </param>
    /// <param name="maxPersonsToShow">Maximum number of persons to show details for per tick. Default is 10.</param>
    /// <returns>This builder for method chaining.</returns>
    public SimulationBuilder WithDebugOutput(bool colored = true, bool showPersonDetails = true,
        int maxPersonsToShow = 10)
    {
        _observers.Add(new DebugObserver(colored, showPersonDetails, maxPersonsToShow));
        return this;
    }

    /// <summary>
    /// Configures the simulation parameters.
    /// </summary>
    /// <param name="config">The simulation configuration.</param>
    /// <returns>This builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="config" /> is <see langword="null" />.
    /// </exception>
    public SimulationBuilder WithSimulationConfig(SimulationConfig config)
    {
        _simulationConfig = config ?? throw new ArgumentNullException(nameof(config));
        return this;
    }

    /// <summary>
    /// Configures the simulation parameters using a builder action.
    /// </summary>
    /// <param name="configure">An action to configure the simulation.</param>
    /// <returns>This builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
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
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="config" /> is <see langword="null" />.
    /// </exception>
    public SimulationBuilder WithModelConfig(StandardModelConfig config)
    {
        _modelConfig = config ?? throw new ArgumentNullException(nameof(config));
        return this;
    }

    /// <summary>
    /// Configures the mathematical model parameters using a builder action.
    /// </summary>
    /// <param name="configure">An action to configure the model.</param>
    /// <returns>This builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="configure" /> is <see langword="null" />.
    /// </exception>
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
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="calculator" /> is <see langword="null" />.
    /// </exception>
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
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="calculator" /> is <see langword="null" />.
    /// </exception>
    public SimulationBuilder WithMigrationCalculator(IMigrationCalculator calculator)
    {
        _migrationCalculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        return this;
    }

    /// <summary>
    /// Sets a random seed for reproducible simulations.
    /// </summary>
    /// <param name="seed">The random seed value.</param>
    /// <returns>This builder for method chaining.</returns>
    public SimulationBuilder WithRandomSeed(int seed)
    {
        _randomSeed = seed;
        return this;
    }

    /// <summary>
    /// Sets a custom stability criteria for determining when the simulation has converged.
    /// </summary>
    /// <param name="stabilityCriteria">The custom stability criteria implementation.</param>
    /// <returns>This builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stabilityCriteria" /> is <see langword="null" />.
    /// </exception>
    public SimulationBuilder WithStabilityCriteria(IStabilityCriteria stabilityCriteria)
    {
        _stabilityCriteria = stabilityCriteria ?? throw new ArgumentNullException(nameof(stabilityCriteria));
        return this;
    }

    /// <summary>
    /// Builds the configured simulation engine.
    /// </summary>
    /// <returns>A configured <see cref="SimulationEngine" /> instance.</returns>
    /// <exception cref="SimulationConfigurationException">
    /// Thrown when the configured <see cref="SimulationConfig" /> is invalid.
    /// </exception>
    public SimulationEngine Build()
    {
        // Create default calculators if not provided
        _attractionCalculator ??= new StandardAttractionCalculator(_modelConfig);
        _migrationCalculator ??= new StandardMigrationCalculator(_modelConfig, _randomSeed);

        // Create default stages if none provided
        var stages = _stages.Count > 0
            ? _stages
            : CreateDefaultStages();

        var validatedConfig = _simulationConfig.Validate();
        var engine = new SimulationEngine(stages, validatedConfig, _stabilityCriteria);

        foreach (var observer in _observers)
            engine.AddObserver(observer);

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