using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Logic.Attraction;
using dotGeoMigrata.Logic.Feedback;
using dotGeoMigrata.Logic.Interfaces;
using dotGeoMigrata.Logic.Migration;
using dotGeoMigrata.Simulation.Configuration;
using dotGeoMigrata.Simulation.Engine;
using dotGeoMigrata.Simulation.Pipeline;

namespace dotGeoMigrata.Simulation.Builders;

/// <summary>
/// Builder for creating and configuring pipeline-based simulation engines with fluent API.
/// </summary>
public sealed class SimulationEngineBuilder
{
    private readonly List<ISimulationObserver> _observers = [];
    private IAttractionCalculator? _attractionCalculator;
    private SimulationConfiguration? _configuration;
    private ISimulationPipeline? _customPipeline;
    private IFeedbackCalculator? _feedbackCalculator;
    private IMigrationCalculator? _migrationCalculator;
    private World? _world;

    /// <summary>
    /// Sets the world to simulate.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when world is null.</exception>
    public SimulationEngineBuilder WithWorld(World world)
    {
        ArgumentNullException.ThrowIfNull(world);
        _world = world;
        return this;
    }

    /// <summary>
    /// Sets the simulation configuration.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when configuration is null.</exception>
    public SimulationEngineBuilder WithConfiguration(SimulationConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _configuration = configuration;
        return this;
    }

    /// <summary>
    /// Sets the attraction calculator to use.
    /// </summary>
    /// <param name="calculator">The attraction calculator.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when calculator is null.</exception>
    public SimulationEngineBuilder WithAttractionCalculator(IAttractionCalculator calculator)
    {
        ArgumentNullException.ThrowIfNull(calculator);
        _attractionCalculator = calculator;
        return this;
    }

    /// <summary>
    /// Uses the enhanced attraction calculator with pull-push model.
    /// </summary>
    /// <returns>This builder instance for chaining.</returns>
    public SimulationEngineBuilder UseEnhancedAttractionCalculator()
    {
        _attractionCalculator = new EnhancedAttractionCalculator();
        return this;
    }

    /// <summary>
    /// Sets the migration calculator to use.
    /// </summary>
    /// <param name="calculator">The migration calculator.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when calculator is null.</exception>
    public SimulationEngineBuilder WithMigrationCalculator(IMigrationCalculator calculator)
    {
        ArgumentNullException.ThrowIfNull(calculator);
        _migrationCalculator = calculator;
        return this;
    }

    /// <summary>
    /// Uses the enhanced migration calculator with sigmoid probability and cost decay.
    /// </summary>
    /// <param name="sigmoidSteepness">Sigmoid steepness coefficient (default: 1.0).</param>
    /// <param name="costSensitivity">Cost sensitivity coefficient (default: 0.01).</param>
    /// <param name="baseMigrationCost">Base migration cost per unit distance (default: 1.0).</param>
    /// <returns>This builder instance for chaining.</returns>
    public SimulationEngineBuilder UseEnhancedMigrationCalculator(
        double sigmoidSteepness = 1.0,
        double costSensitivity = 0.01,
        double baseMigrationCost = 1.0)
    {
        _migrationCalculator = new EnhancedMigrationCalculator
        {
            SigmoidSteepness = sigmoidSteepness,
            CostSensitivity = costSensitivity,
            BaseMigrationCost = baseMigrationCost
        };
        return this;
    }

    /// <summary>
    /// Sets the feedback calculator to use.
    /// </summary>
    /// <param name="calculator">The feedback calculator.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when calculator is null.</exception>
    public SimulationEngineBuilder WithFeedbackCalculator(IFeedbackCalculator calculator)
    {
        ArgumentNullException.ThrowIfNull(calculator);
        _feedbackCalculator = calculator;
        return this;
    }

    /// <summary>
    /// Uses the enhanced feedback calculator with comprehensive factor update rules.
    /// </summary>
    /// <param name="feedbackRules">Optional factor-specific feedback rules.</param>
    /// <returns>This builder instance for chaining.</returns>
    public SimulationEngineBuilder UseEnhancedFeedbackCalculator(
        IEnumerable<FactorFeedbackRule>? feedbackRules = null)
    {
        _feedbackCalculator = new EnhancedFeedbackCalculator(feedbackRules);
        return this;
    }

    /// <summary>
    /// Sets a custom simulation pipeline.
    /// </summary>
    /// <param name="pipeline">The custom pipeline.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when pipeline is null.</exception>
    public SimulationEngineBuilder WithCustomPipeline(ISimulationPipeline pipeline)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        _customPipeline = pipeline;
        return this;
    }

    /// <summary>
    /// Adds an observer to monitor simulation progress.
    /// </summary>
    /// <param name="observer">The observer to add.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when observer is null.</exception>
    public SimulationEngineBuilder AddObserver(ISimulationObserver observer)
    {
        ArgumentNullException.ThrowIfNull(observer);
        _observers.Add(observer);
        return this;
    }

    /// <summary>
    /// Adds a console observer for monitoring simulation progress.
    /// </summary>
    /// <returns>This builder instance for chaining.</returns>
    public SimulationEngineBuilder AddConsoleObserver()
    {
        _observers.Add(new ConsoleSimulationObserver());
        return this;
    }

    /// <summary>
    /// Builds the pipeline simulation engine with the configured settings.
    /// </summary>
    /// <returns>The configured pipeline simulation engine.</returns>
    /// <exception cref="InvalidOperationException">Thrown when required components are not configured.</exception>
    public SimulationEngine Build()
    {
        if (_world is null)
            throw new InvalidOperationException("World must be configured before building the simulation engine.");

        if (_configuration is null)
            throw new InvalidOperationException(
                "Configuration must be configured before building the simulation engine.");

        SimulationEngine engine;

        if (_customPipeline is not null)
        {
            engine = new SimulationEngine(_world, _configuration, _customPipeline);
        }
        else
        {
            // Use default calculators if not specified
            var attractionCalc = _attractionCalculator ?? new EnhancedAttractionCalculator();
            var migrationCalc = _migrationCalculator ?? new EnhancedMigrationCalculator();
            var feedbackCalc = _feedbackCalculator ?? new EnhancedFeedbackCalculator();

            // Set feedback smoothing factor from configuration
            feedbackCalc.SmoothingFactor = _configuration.FeedbackSmoothingFactor;

            engine = new SimulationEngine(
                _world,
                _configuration,
                attractionCalc,
                migrationCalc,
                feedbackCalc);
        }

        // Add observers
        foreach (var observer in _observers)
            engine.AddObserver(observer);

        return engine;
    }
}