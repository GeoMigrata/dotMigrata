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
/// Builder for creating and configuring simulation engines with fluent API.
/// </summary>
public sealed class SimulationEngineBuilder
{
    private readonly List<ISimulationObserver> _observers = [];
    private IAttractionCalculator? _attractionCalculator;
    private SimulationConfiguration? _configuration;
    private ISimulationPipeline? _customPipeline;
    private IFeedbackCalculator? _feedbackCalculator;
    private IMigrationCalculator? _migrationCalculator;
    private bool _usePipelineEngine = true;
    private World? _world;

    /// <summary>
    /// Sets the world to simulate.
    /// </summary>
    /// <param name="world">The world.</param>
    /// <returns>This builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when world is null.</exception>
    public SimulationEngineBuilder WithWorld(World world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
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
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        return this;
    }

    /// <summary>
    /// Uses the original (non-pipeline) simulation engine.
    /// </summary>
    /// <returns>This builder instance for chaining.</returns>
    /// <remarks>
    /// This method is deprecated. Use <see cref="UsePipelineEngine" /> and enhanced calculators instead.
    /// The original engine lacks the modularity and enhanced algorithms of the pipeline version.
    /// </remarks>
    [Obsolete(
        "Use UsePipelineEngine() with enhanced calculators for new projects. This method is maintained for backward compatibility only.")]
    public SimulationEngineBuilder UseOriginalEngine()
    {
        _usePipelineEngine = false;
        return this;
    }

    /// <summary>
    /// Uses the pipeline-based simulation engine.
    /// </summary>
    /// <returns>This builder instance for chaining.</returns>
    public SimulationEngineBuilder UsePipelineEngine()
    {
        _usePipelineEngine = true;
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
        _attractionCalculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        return this;
    }

    /// <summary>
    /// Uses the original attraction calculator.
    /// </summary>
    /// <returns>This builder instance for chaining.</returns>
    /// <remarks>
    /// This method is deprecated. Use <see cref="UseEnhancedAttractionCalculator" /> instead.
    /// The enhanced version implements the pull-push factor model per LogicModel.md specification.
    /// </remarks>
    [Obsolete(
        "Use UseEnhancedAttractionCalculator() for new projects. This method is maintained for backward compatibility only.")]
    public SimulationEngineBuilder UseOriginalAttractionCalculator()
    {
        _attractionCalculator = new AttractionCalculator();
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
        _migrationCalculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        return this;
    }

    /// <summary>
    /// Uses the original migration calculator.
    /// </summary>
    /// <returns>This builder instance for chaining.</returns>
    /// <remarks>
    /// This method is deprecated. Use <see cref="UseEnhancedMigrationCalculator" /> instead.
    /// The enhanced version implements sigmoid probability, cost decay, and capacity constraints per LogicModel.md.
    /// </remarks>
    [Obsolete(
        "Use UseEnhancedMigrationCalculator() for new projects. This method is maintained for backward compatibility only.")]
    public SimulationEngineBuilder UseOriginalMigrationCalculator()
    {
        _migrationCalculator = new MigrationCalculator();
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
        _feedbackCalculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        return this;
    }

    /// <summary>
    /// Uses the original feedback calculator.
    /// </summary>
    /// <returns>This builder instance for chaining.</returns>
    /// <remarks>
    /// This method is deprecated. Use <see cref="UseEnhancedFeedbackCalculator" /> instead.
    /// The enhanced version implements multiple feedback mechanisms including per-capita resources,
    /// price elasticity, and externalities per LogicModel.md specification.
    /// </remarks>
    [Obsolete(
        "Use UseEnhancedFeedbackCalculator() for new projects. This method is maintained for backward compatibility only.")]
    public SimulationEngineBuilder UseOriginalFeedbackCalculator()
    {
        _feedbackCalculator = new FeedbackCalculator();
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
        _customPipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        _usePipelineEngine = true;
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
    /// Builds the simulation engine with the configured settings.
    /// </summary>
    /// <returns>The configured simulation engine (either original or pipeline-based).</returns>
    /// <exception cref="InvalidOperationException">Thrown when required components are not configured.</exception>
    public object Build()
    {
        if (_world is null)
            throw new InvalidOperationException("World must be configured before building the simulation engine.");

        if (_configuration is null)
            throw new InvalidOperationException(
                "Configuration must be configured before building the simulation engine.");

        object engine;

        if (_usePipelineEngine)
            engine = BuildPipelineEngine();
        else
            engine = BuildOriginalEngine();


        // Add observers
        switch (engine)
        {
#pragma warning disable CS0618 // Type is obsolete - intentional for backward compatibility
            case SimulationEngine originalEngine:
            {
                foreach (var observer in _observers)
                    originalEngine.AddObserver(observer);
                break;
            }
#pragma warning restore CS0618
            case PipelineSimulationEngine pipelineEngine:
            {
                foreach (var observer in _observers)
                    pipelineEngine.AddObserver(observer);
                break;
            }
        }

        return engine;
    }

    private PipelineSimulationEngine BuildPipelineEngine()
    {
        if (_customPipeline != null) return new PipelineSimulationEngine(_world!, _configuration!, _customPipeline);

        // Use default calculators if not specified
        var attractionCalc = _attractionCalculator ?? new EnhancedAttractionCalculator();
        var migrationCalc = _migrationCalculator ?? new EnhancedMigrationCalculator();
        var feedbackCalc = _feedbackCalculator ?? new EnhancedFeedbackCalculator();

        // Set feedback smoothing factor from configuration
        feedbackCalc.SmoothingFactor = _configuration!.FeedbackSmoothingFactor;

        return new PipelineSimulationEngine(
            _world!,
            _configuration!,
            attractionCalc,
            migrationCalc,
            feedbackCalc);
    }

#pragma warning disable CS0618 // Type is obsolete - intentional for backward compatibility
    private SimulationEngine BuildOriginalEngine()
    {
        // Original engine doesn't use the interface-based calculators directly
        // It creates its own instances internally
        return new SimulationEngine(_world!, _configuration!);
#pragma warning restore CS0618
    }
}