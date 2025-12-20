using dotMigrata.Core.Entities;
using dotMigrata.Core.Values;
using dotMigrata.Logic.Calculators;
using dotMigrata.Logic.Interfaces;
using dotMigrata.Logic.Models;
using dotMigrata.Simulation.Engine;
using dotMigrata.Simulation.Events;
using dotMigrata.Simulation.Events.Effects;
using dotMigrata.Simulation.Events.Enums;
using dotMigrata.Simulation.Events.Interfaces;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;
using dotMigrata.Simulation.Pipeline;
using Microsoft.Extensions.Logging;

namespace dotMigrata.Simulation.Builders;

/// <summary>
/// Provides a fluent API for creating and configuring simulation engines.
/// </summary>
/// <remarks>
/// Provides a streamlined API for setting up simulations with sensible defaults.
/// </remarks>
public sealed class SimulationBuilder
{
    private readonly List<ISimulationEvent> _events = [];
    private readonly List<ISimulationObserver> _observers = [];
    private readonly List<ISimulationStage> _stages = [];
    private IAttractionCalculator? _attractionCalculator;
    private int _defaultEventInterval = 1;
    private int? _eventParallelism;
    private ILogger<SimulationEngine>? _logger;
    private IMigrationCalculator? _migrationCalculator;
    private StandardModelConfig _modelConfig = StandardModelConfig.Default;
    private int? _randomSeed;
    private SimulationConfig _simulationConfig = SimulationConfig.Default;
    private IStabilityCriteria? _stabilityCriteria;
    private bool _useParallelEvents = true;

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
    /// Sets a logger for structured logging of simulation events.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging.</param>
    /// <returns>This builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="logger" /> is <see langword="null" />.
    /// </exception>
    public SimulationBuilder WithLogger(ILogger<SimulationEngine> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        return this;
    }

    /// <summary>
    /// Adds a simulation event to the pipeline.
    /// </summary>
    /// <param name="evt">The event to add.</param>
    /// <returns>This builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="evt" /> is <see langword="null" />.
    /// </exception>
    public SimulationBuilder WithEvent(ISimulationEvent evt)
    {
        ArgumentNullException.ThrowIfNull(evt);
        _events.Add(evt);
        return this;
    }

    /// <summary>
    /// Adds a factor change event with fluent configuration.
    /// </summary>
    /// <param name="displayName">The display name of the event.</param>
    /// <param name="factor">The factor to modify.</param>
    /// <param name="valueSpecification">The target value specification.</param>
    /// <param name="trigger">The trigger determining when this event fires.</param>
    /// <param name="applicationType">How the value change is applied.</param>
    /// <param name="duration">Duration over which the effect is applied.</param>
    /// <param name="cityFilter">Optional filter for which cities are affected.</param>
    /// <param name="description">Optional description of the event.</param>
    /// <returns>This builder for method chaining.</returns>
    public SimulationBuilder WithFactorChange(
        string displayName,
        FactorDefinition factor,
        UnitValueSpec valueSpecification,
        IEventTrigger trigger,
        EffectApplicationType applicationType = EffectApplicationType.Absolute,
        EffectDuration? duration = null,
        Func<City, bool>? cityFilter = null,
        string? description = null)
    {
        var effect = new FactorChangeEffect(
            factor,
            valueSpecification,
            applicationType,
            duration ?? EffectDuration.Instant,
            cityFilter,
            _randomSeed);

        var evt = new SimulationEvent(displayName, trigger, effect, description);
        _events.Add(evt);

        return this;
    }

    /// <summary>
    /// Sets the default interval for periodic events.
    /// </summary>
    /// <param name="interval">The interval in ticks between executions.</param>
    /// <returns>This builder for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="interval" /> is less than 1.
    /// </exception>
    public SimulationBuilder WithEventInterval(int interval)
    {
        if (interval < 1)
            throw new ArgumentOutOfRangeException(nameof(interval), "Event interval must be at least 1.");
        _defaultEventInterval = interval;
        return this;
    }

    /// <summary>
    /// Configures whether events should execute in parallel for better performance.
    /// </summary>
    /// <param name="useParallel">True to enable parallel event execution; false for sequential.</param>
    /// <param name="maxDegreeOfParallelism">
    /// Optional maximum degree of parallelism. If null, uses system default.
    /// </param>
    /// <returns>This builder for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxDegreeOfParallelism" /> is less than or equal to 0.
    /// </exception>
    public SimulationBuilder WithParallelEvents(bool useParallel = true, int? maxDegreeOfParallelism = null)
    {
        if (maxDegreeOfParallelism is <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxDegreeOfParallelism),
                "Max degree of parallelism must be greater than 0.");

        _useParallelEvents = useParallel;
        _eventParallelism = maxDegreeOfParallelism;
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
        var engine = new SimulationEngine(stages, validatedConfig, _stabilityCriteria, _logger);

        foreach (var observer in _observers)
            engine.AddObserver(observer);

        return engine;
    }

    private List<ISimulationStage> CreateDefaultStages()
    {
        var stages = new List<ISimulationStage>
        {
            new MigrationDecisionStage(_migrationCalculator!, _attractionCalculator!),
            new MigrationExecutionStage()
        };

        // Add event stage if events are configured
        if (_events.Count > 0)
            stages.Add(new EventStage(_events, _useParallelEvents, _eventParallelism));

        return stages;
    }
}