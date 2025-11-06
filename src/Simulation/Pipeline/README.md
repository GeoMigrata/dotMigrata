# Pipeline Architecture Documentation

## Overview

The new simulation engine uses a modern pipeline architecture, providing enhanced extensibility, modularity, and low
coupling. The pipeline architecture decomposes the simulation workflow into independent stages, each responsible for
specific functionality.

## Core Components

### ISimulationStage - Simulation Stage Interface

Defines the contract for a single stage in the pipeline:

```csharp
public interface ISimulationStage
{
    string Name { get; }           // Stage name
    int Order { get; }             // Execution order (ascending)
    SimulationStageResult Execute(SimulationContext context);
}
```

### SimulationContext - Simulation Context

Context object that flows through pipeline stages:

```csharp
public sealed class SimulationContext
{
    public required World World { get; init; }           // World state
    public required SimulationState State { get; init; } // Simulation state
    public required Random Random { get; init; }         // Random generator
    public Dictionary<string, object> SharedData { get; init; }  // Shared data
}
```

### ISimulationPipeline - Simulation Pipeline Interface

Manages and executes the stage collection:

```csharp
public interface ISimulationPipeline
{
    void AddStage(ISimulationStage stage);
    bool RemoveStage(ISimulationStage stage);
    bool Execute(SimulationContext context);
    IReadOnlyList<ISimulationStage> Stages { get; }
}
```

## Default Pipeline Stages

### 1. AttractionStage (Order: 100)

**Purpose**: Calculate city attraction for all population groups

**Input**:

- World (cities, factors, population group definitions)
- IAttractionCalculator (attraction calculator)

**Output**:

- SharedData["Attractions"] - Attraction results dictionary
    - Key: Population group name
    - Value: List of attraction results for all cities

**Calculators**:

- `AttractionCalculator` - Original implementation
- `EnhancedAttractionCalculator` - Enhanced (pull-push model)

### 2. MigrationStage (Order: 200)

**Purpose**: Calculate migration flows based on attraction differences

**Input**:

- SharedData["Attractions"] - Attraction results from previous stage
- IMigrationCalculator (migration calculator)

**Output**:

- SharedData["MigrationFlows"] - List of migration flows

**Calculators**:

- `MigrationCalculator` - Original implementation
- `EnhancedMigrationCalculator` - Enhanced (sigmoid probability, cost decay)

### 3. MigrationApplicationStage (Order: 300)

**Purpose**: Apply migration flows, update city populations

**Input**:

- SharedData["MigrationFlows"] - Migration flow list

**Output**:

- SharedData["PreviousPopulations"] - City population dictionary before migration
- SharedData["TotalMigrants"] - Total migrant count

**Logic**:

1. Record pre-migration city populations
2. Group by source city, reduce population (emigration)
3. Group by destination city, increase population (immigration)
4. Save results for next stage

### 4. FeedbackStage (Order: 400)

**Purpose**: Update city factors based on population changes

**Input**:

- SharedData["PreviousPopulations"] - Pre-migration population
- IFeedbackCalculator (feedback calculator)

**Output**:

- Updated city factor values

**Calculators**:

- `FeedbackCalculator` - Original implementation
- `EnhancedFeedbackCalculator` - Enhanced (multiple feedback mechanisms)

## Using the Pipeline Engine

### Method 1: SimulationEngineBuilder (Recommended)

```csharp
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(config)
    .UseEnhancedAttractionCalculator()
    .UseEnhancedMigrationCalculator(
        sigmoidSteepness: 1.0,
        costSensitivity: 0.01)
    .UseEnhancedFeedbackCalculator(feedbackRules)
    .AddConsoleObserver()
    .Build();

if (engine is PipelineSimulationEngine pipelineEngine)
{
    pipelineEngine.Run();
}
```

### Method 2: Direct PipelineSimulationEngine Creation

```csharp
var attractionCalc = new EnhancedAttractionCalculator();
var migrationCalc = new EnhancedMigrationCalculator();
var feedbackCalc = new EnhancedFeedbackCalculator(feedbackRules);

var engine = new PipelineSimulationEngine(
    world,
    config,
    attractionCalc,
    migrationCalc,
    feedbackCalc);

engine.AddObserver(new ConsoleSimulationObserver());
engine.Run();
```

### Method 3: Custom Pipeline

```csharp
var pipeline = new SimulationPipeline();

// Add custom stages
pipeline.AddStage(new AttractionStage(new EnhancedAttractionCalculator()));
pipeline.AddStage(new CustomPreprocessingStage());  // Custom stage
pipeline.AddStage(new MigrationStage(new EnhancedMigrationCalculator()));
pipeline.AddStage(new MigrationApplicationStage());
pipeline.AddStage(new CustomAnalysisStage());       // Custom stage
pipeline.AddStage(new FeedbackStage(new EnhancedFeedbackCalculator()));

var engine = new PipelineSimulationEngine(world, config, pipeline);
engine.Run();
```

## Creating Custom Stages

Implement the `ISimulationStage` interface:

```csharp
public class CustomAnalysisStage : ISimulationStage
{
    public string Name => "Custom Analysis";
    public int Order => 350; // Execute between application and feedback
    public SimulationStageResult Execute(SimulationContext context)
    {
        try
        {
            // Get migration flows from shared data
            if (context.SharedData.TryGetValue("MigrationFlows", 
                out var flowsObj) && flowsObj is List<MigrationFlow> flows)
            {
                // Perform custom analysis
                var analysis = AnalyzeMigrationPatterns(flows);

                // Store results in shared data
                context.SharedData["MigrationAnalysis"] = analysis;

                return SimulationStageResult.Successful(
                    "Analysis completed", analysis);
            }

            return SimulationStageResult.Failed("Migration flows not found");
        }
        catch (Exception ex)
        {
            return SimulationStageResult.Failed($"Analysis failed: {ex.Message}");
        }
    }
}
```

## Advantages

### 1. High Modularity

- Single-responsibility stages
- Independent development, testing, and maintenance
- Easy to understand and debug

### 2. Extensibility

- Add new stages effortlessly
- Control execution order via Order property
- Insert custom logic anywhere

### 3. Low Coupling

- Stages communicate via SharedData
- No direct dependencies between stages
- Use interfaces instead of concrete implementations

### 4. Flexible Configuration

- Choose different calculator implementations
- Support fully custom pipelines
- Builder pattern simplifies configuration

### 5. Testability

- Each stage can be unit tested independently
- Mock context and shared data
- Simplified integration testing

## Backward Compatibility

The original `SimulationEngine` remains intact and functional. Users can:

- Continue using `SimulationEngine` (legacy non-pipeline version)
- Migrate to `PipelineSimulationEngine` (new pipeline version)
- Choose flexibly via `SimulationEngineBuilder`

## Best Practices

1. **Use Builder Pattern**: Configure engines via `SimulationEngineBuilder`
2. **Choose Appropriate Calculators**: Select original or enhanced based on requirements
3. **Define Clear Stage Order**: Use multiples of 100 for Order values to allow insertion
4. **Use SharedData Wisely**: Use descriptive keys, avoid conflicts
5. **Handle Errors Properly**: Handle exceptions in custom stages, return failure results
6. **Document Custom Stages**: Write clear documentation for custom stages

## Performance Considerations

- Pipeline overhead is minimal
- Performance depends on calculator implementations
- SharedData uses dictionary, O(1) lookup
- Recommend profiling tools for large-scale simulations

## Complete Usage Example

```csharp
// 1. Prepare data
var world = CreateWorld();
var config = new SimulationConfiguration 
{ 
    MaxSteps = 100,
    FeedbackSmoothingFactor = 0.2
};

// 2. Define feedback rules (optional)
var feedbackRules = new List<FactorFeedbackRule>
{
    new() { 
        Factor = housingPriceFactor, 
        FeedbackType = FeedbackType.PriceCost,
        Elasticity = 0.3 
    },
    new() { 
        Factor = pollutionFactor, 
        FeedbackType = FeedbackType.NegativeExternality,
        ExternalityCoefficient = 0.0001 
    }
};

// 3. Build engine
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(config)
    .UseEnhancedAttractionCalculator()
    .UseEnhancedMigrationCalculator(sigmoidSteepness: 1.5)
    .UseEnhancedFeedbackCalculator(feedbackRules)
    .AddConsoleObserver()
    .Build() as PipelineSimulationEngine;

// 4. Run simulation
engine?.Run();

// 5. Analyze results
var finalState = engine?.State;
Console.WriteLine($"Simulation completed: {finalState?.CurrentStep} steps");
```