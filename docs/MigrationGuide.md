# Migration Guide: From Legacy to Pipeline Architecture

This guide helps you migrate existing code from the original `SimulationEngine` to the new `PipelineSimulationEngine`.

## Why Migrate?

The new pipeline architecture provides:

- ✅ **Enhanced Extensibility** - Add custom stages effortlessly
- ✅ **Clearer Structure** - Single-responsibility stages
- ✅ **Advanced Algorithms** - Based on LogicModel.md specification
- ✅ **Flexible Configuration** - Builder pattern simplifies setup
- ✅ **Backward Compatible** - Existing code continues to work

## Migration Steps

### Step 1: Understand the Differences

#### Legacy Code (Still Supported)

```csharp
var engine = new SimulationEngine(world, configuration);
engine.AddObserver(new ConsoleSimulationObserver());
engine.Run();
```

#### New Architecture (Recommended)

```csharp
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(configuration)
    .UseEnhancedAttractionCalculator()
    .UseEnhancedMigrationCalculator()
    .UseEnhancedFeedbackCalculator()
    .AddConsoleObserver()
    .Build() as PipelineSimulationEngine;

engine?.Run();
```

### Step 2: Gradual Migration

#### Minimal Changes

Keep the original algorithms while using the pipeline architecture:

```csharp
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(configuration)
    .UseOriginalAttractionCalculator()  // Legacy algorithm
    .UseOriginalMigrationCalculator()   // Legacy algorithm
    .UseOriginalFeedbackCalculator()    // Legacy algorithm
    .AddConsoleObserver()
    .Build() as PipelineSimulationEngine;
```

#### Incremental Upgrade

Transition components one at a time:

```csharp
// Step 1: Upgrade attraction calculation only
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(configuration)
    .UseEnhancedAttractionCalculator()     // ✅ New algorithm
    .UseOriginalMigrationCalculator()      // ⏸️ Keep original
    .UseOriginalFeedbackCalculator()       // ⏸️ Keep original
    .Build() as PipelineSimulationEngine;

// Validate results, then continue with other components...
```

#### Complete Migration

```csharp
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(configuration)
    .UseEnhancedAttractionCalculator()
    .UseEnhancedMigrationCalculator(
        sigmoidSteepness: 1.0,      // Custom parameters
        costSensitivity: 0.01,
        baseMigrationCost: 1.0)
    .UseEnhancedFeedbackCalculator(feedbackRules)
    .AddConsoleObserver()
    .Build() as PipelineSimulationEngine;
```

### Step 3: Configure New Features

#### Add Population Group Attributes

Legacy:

```csharp
var groupDef = new PopulationGroupDefinition(sensitivities)
{
    DisplayName = "Young Professionals",
    MovingWillingness = 0.3,
    RetentionRate = 0.05
};
```

Enhanced (Optional attributes):

```csharp
var groupDef = new PopulationGroupDefinition(sensitivities)
{
    DisplayName = "Young Professionals",
    MovingWillingness = 0.3,
    RetentionRate = 0.05,
    SensitivityScaling = 1.2,          // New: Sensitivity coefficient
    AttractionThreshold = 0.1,         // New: Attraction threshold
    MinimumAcceptableAttraction = 0.2  // New: Minimum acceptable attraction
};
```

#### Add City Capacity

Legacy:

```csharp
var city = new City(factorValues, populationValues)
{
    DisplayName = "Beijing",
    Location = coordinate,
    Area = 16410
};
```

Enhanced (Optional capacity):

```csharp
var city = new City(factorValues, populationValues)
{
    DisplayName = "Beijing",
    Location = coordinate,
    Area = 16410,
    Capacity = 2_000_000  // New: Maximum capacity of 2 million
};
```

#### Configure Feedback Rules

```csharp
var feedbackRules = new List<FactorFeedbackRule>
{
    // Housing price: Price elasticity
    new() 
    { 
        Factor = housingPriceFactor,
        FeedbackType = FeedbackType.PriceCost,
        Elasticity = 0.3
    },

    // Pollution: Negative externality
    new() 
    { 
        Factor = pollutionFactor,
        FeedbackType = FeedbackType.NegativeExternality,
        ExternalityCoefficient = 0.0001
    },

    // Economic output: Positive externality
    new() 
    { 
        Factor = economicOutputFactor,
        FeedbackType = FeedbackType.PositiveExternality,
        SaturationPoint = 1_000_000
    },

    // Healthcare: Per-capita resource
    new() 
    { 
        Factor = healthcareFactor,
        FeedbackType = FeedbackType.PerCapitaResource
    }
};

var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(configuration)
    .UseEnhancedFeedbackCalculator(feedbackRules)  // Pass rules
    // ...
    .Build();
```

### Step 4: Custom Pipeline (Advanced)

For complete control over the simulation workflow:

```csharp
var pipeline = new SimulationPipeline();

// Add standard stages
pipeline.AddStage(new AttractionStage(new EnhancedAttractionCalculator()));

// Add custom stages
pipeline.AddStage(new MyCustomPreprocessingStage { Order = 150 });

pipeline.AddStage(new MigrationStage(new EnhancedMigrationCalculator()));
pipeline.AddStage(new MigrationApplicationStage());

// Add custom analysis stage
pipeline.AddStage(new MyCustomAnalysisStage { Order = 350 });

pipeline.AddStage(new FeedbackStage(new EnhancedFeedbackCalculator()));

// Use custom pipeline
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(configuration)
    .WithCustomPipeline(pipeline)
    .Build() as PipelineSimulationEngine;
```

## Frequently Asked Questions

### Q1: Does legacy code still work?

**A:** Yes. `SimulationEngine` remains unchanged. All existing code works without modification.

### Q2: Are results identical between old and new?

**A:** Not exactly. Enhanced implementations use more precise algorithms (per LogicModel.md), but using `UseOriginal*`
methods maintains legacy behavior.

### Q3: Must all components be upgraded simultaneously?

**A:** No. You can mix original and enhanced calculators.

### Q4: How to validate migration results?

**A:**

1. Run with original calculators and record results
2. Switch to enhanced versions one at a time
3. Compare differences
4. Test with small datasets
5. Add logging to observe stage outputs

### Q5: Performance impact?

**A:** Pipeline overhead is minimal. Enhanced algorithms may be slightly slower due to added complexity, but differences
are typically negligible.

## Migration Checklist

- [ ] Back up existing code
- [ ] Review new architecture and algorithms
- [ ] Choose migration strategy (complete/incremental/hybrid)
- [ ] Update population group definitions (add new attributes)
- [ ] Update city definitions (add capacity)
- [ ] Configure feedback rules (if using enhanced feedback)
- [ ] Test new version
- [ ] Compare results
- [ ] Update documentation
- [ ] Deploy new version

## Example: Complete Migration

### Before

```csharp
// Create world
var world = new World(cities, factorDefinitions, populationGroupDefinitions)
{
    DisplayName = "Major Chinese Cities"
};

// Create configuration
var config = new SimulationConfiguration
{
    MaxSteps = 100,
    FeedbackSmoothingFactor = 0.3
};

// Create and run engine
var engine = new SimulationEngine(world, config);
engine.AddObserver(new ConsoleSimulationObserver());
engine.Run();

// Get results
var finalState = engine.State;
Console.WriteLine($"Simulation completed: {finalState.CurrentStep} steps");
```

### After

```csharp
// Create world (with capacity)
var cities = CreateCitiesWithCapacity();  // Added capacity property
var world = new World(cities, factorDefinitions, populationGroupDefinitions)
{
    DisplayName = "Major Chinese Cities"
};

// Create configuration (same as before)
var config = new SimulationConfiguration
{
    MaxSteps = 100,
    FeedbackSmoothingFactor = 0.3
};

// Define feedback rules
var feedbackRules = new List<FactorFeedbackRule>
{
    new() { Factor = housingPriceFactor, FeedbackType = FeedbackType.PriceCost },
    new() { Factor = pollutionFactor, FeedbackType = FeedbackType.NegativeExternality }
};

// Build engine using Builder
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(config)
    .UseEnhancedAttractionCalculator()
    .UseEnhancedMigrationCalculator(sigmoidSteepness: 1.0, costSensitivity: 0.01)
    .UseEnhancedFeedbackCalculator(feedbackRules)
    .AddConsoleObserver()
    .Build() as PipelineSimulationEngine;

// Run simulation
engine?.Run();

// Get results
var finalState = engine?.State;
Console.WriteLine($"Simulation completed: {finalState?.CurrentStep} steps");

// New feature: Access pipeline
var pipeline = engine?.Pipeline;
Console.WriteLine($"Pipeline stages: {pipeline?.Stages.Count}");
```

## Performance Optimization

Recommendations after migrating:

1. **Adjust Sigmoid Steepness**: Tune `SigmoidSteepness` based on data characteristics
2. **Optimize Cost Coefficients**: Adjust `CostSensitivity` and `BaseMigrationCost`
3. **Set Capacity Wisely**: Only set `Capacity` for cities that need limits
4. **Streamline Feedback Rules**: Add rules only for factors requiring feedback
5. **Adjust Smoothing Factor**: Larger `SmoothingFactor` reduces volatility

## Getting Help

- See `/src/Simulation/Pipeline/README.md` - Pipeline architecture
- See `/src/Logic/EnhancedAlgorithms.md` - Enhanced algorithms
- See `/src/Logic/LogicModel.md` - Algorithm design specification
- Submit GitHub Issues for support

## Summary

Benefits of migrating to the new architecture:

- ✅ Modern design patterns
- ✅ Enhanced capabilities
- ✅ Better extensibility
- ✅ More precise algorithms
- ✅ Maintained backward compatibility

Recommended approach: Incremental migration, validating each component upgrade.