# Simulation Engine Documentation

This directory contains the simulation engine that orchestrates the population migration simulation using a modern
pipeline architecture.

## Overview

The Simulation layer provides the orchestration framework that runs the step-by-step simulation, managing state,
configuration, and observation of the simulation process through a flexible pipeline architecture.

## Components

### 1. SimulationEngine

The main orchestrator that coordinates all simulation activities using a configurable pipeline of stages.

### 2. SimulationConfiguration

Configuration parameters that control simulation behavior.

### 3. SimulationState

Tracks the current state and statistics of a running simulation.

### 4. ISimulationObserver

Observer interface for monitoring simulation progress.

### 5. Pipeline Architecture

A flexible system of stages that allows customization and extension of simulation workflow.

---

## SimulationEngine Design

### Architecture

The `SimulationEngine` follows the **Pipeline Pattern** and **Observer Pattern**, executing a series of configurable
stages:

- `AttractionStage` - Computes city attractiveness using an `IAttractionCalculator`
- `MigrationStage` - Determines migration flows using an `IMigrationCalculator`
- `MigrationApplicationStage` - Applies migration flows to update city populations
- `FeedbackStage` - Updates city factors using an `IFeedbackCalculator`

### Key Responsibilities

1. **Pipeline Orchestration**: Executes configurable simulation stages in sequence
2. **State Management**: Maintains and updates simulation state
3. **Observer Notification**: Notifies observers of simulation events
4. **Stabilization Detection**: Monitors when the system reaches equilibrium

---

## Simulation Flow

### Initialization

```csharp
// 1. Create World with cities, factors, and population groups
var world = new World(cities, factorDefinitions);

// 2. Create SimulationConfiguration with parameters
var config = new SimulationConfiguration { MaxSteps = 100 };

// 3. Build engine using SimulationEngineBuilder
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(config)
    .UseEnhancedAttractionCalculator()
    .UseEnhancedMigrationCalculator()
    .UseEnhancedFeedbackCalculator()
    .AddConsoleObserver()
    .Build();

// 4. Run simulation
engine.Run();
```

### Main Loop (Run Method)

```csharp
void Run()
{
    // 1. Initialize
    NotifyObservers_SimulationStarted()
    
    // 2. Main simulation loop
    while (!IsCompleted && CurrentStep < MaxSteps)
    {
        Step()  // Execute one simulation step
        
        // Check for stabilization (if enabled)
        if (CheckStabilization())
        {
            MarkStabilized()
            break
        }
    }
    
    // 3. Finalize
    MarkCompleted()
    NotifyObservers_SimulationCompleted()
}
```

### Single Step Execution

Each call to `Step()` executes the pipeline stages:

```
┌─────────────────────────────────────────────────┐
│ Step N                                          │
├─────────────────────────────────────────────────┤
│                                                 │
│  Pipeline.Execute(context)                      │
│                                                 │
│  1. AttractionStage                             │
│     ┌─────────────────────────────────────┐     │
│     │ Calculate Attractions               │     │
│     │  - Uses IAttractionCalculator       │     │
│     │  - Produces AttractionResults       │     │
│     │  - Stores in context.SharedData     │     │
│     └─────────────────────────────────────┘     │
│              ↓                                  │
|  2. MigrationStage                              |  
│     ┌─────────────────────────────────────┐     │
│     │ Calculate Migration Flows           │     │
│     │  - Uses IMigrationCalculator        │     │
│     │  - Retrieves attractions from       │     │
│     │    context.SharedData               │     │
│     │  - Produces MigrationFlows          │     │
│     └─────────────────────────────────────┘     │
│              ↓                                  │
│  3. MigrationApplicationStage                   │
│     ┌─────────────────────────────────────┐     │
│     │ Apply Migration Flows               │     │
│     │  - Update Population Counts         │     │
│     │  - Track migration statistics       │     │
│     └─────────────────────────────────────┘     │
│              ↓                                  │
│  4. FeedbackStage                               │
│     ┌─────────────────────────────────────┐     │
│     │ Update City Factors                 │     │
│     │  - Uses IFeedbackCalculator         │     │
│     │  - Adjust factors based on pop.     │     │
│     └─────────────────────────────────────┘     │
│              ↓                                  │
│  5. Update state and notify                     │
│     ┌─────────────────────────────────────┐     │
│     │ Advance Step Counter                │     │
│     │ Record Statistics                   │     │
│     │ Notify Observers                    │     │
│     └─────────────────────────────────────┘     │
│                                                 │
└─────────────────────────────────────────────────┘
```

### Detailed Step Algorithm

```csharp
void Step()
{
    if (IsCompleted)
        throw InvalidOperationException
    
    allMigrationFlows = new List<MigrationFlow>()
    
    // Phase 1: Calculate all migrations
    // Create simulation context for pipeline
    var context = new SimulationContext
    {
        World = world,
        State = state,
        Random = random,
        SharedData = new Dictionary<string, object>()
    }
    
    // Execute pipeline stages in order
    Pipeline.Execute(context)
    
    // Extract results from context
    var migrationFlows = context.SharedData["MigrationFlows"]
    var totalMigrants = context.SharedData["TotalMigrants"]
    
    // Update state
    state.AdvanceStep(totalMigrants)
    RaiseStepCompletedEvent()
    NotifyObservers_StepCompleted()
}
```

---

## Configuration

### SimulationConfiguration

Controls simulation behavior with the following parameters:

#### MaxSteps

- **Type**: `int` (required)
- **Range**: > 0
- **Description**: Maximum number of simulation steps to run
- **Example**: `MaxSteps = 100`

#### StabilizationThreshold

- **Type**: `double`
- **Range**: [0, 1]
- **Default**: 0.01
- **Description**: Migration rate threshold below which simulation is considered stabilized
- **Formula**: `migrationRate = totalMigrants / totalPopulation`
- **Example**: If threshold is 0.01, simulation stabilizes when < 1% of population migrates per step

#### CheckStabilization

- **Type**: `bool`
- **Default**: `true`
- **Description**: Whether to check for stabilization and potentially end early
- **Example**: `CheckStabilization = true`

#### FeedbackSmoothingFactor

- **Type**: `double`
- **Range**: [0, 1]
- **Default**: 0.3
- **Description**: Controls how gradually city factors change
    - 0.0 = instant changes
    - 1.0 = no changes
    - 0.3 = gradual changes (recommended)

#### RandomSeed

- **Type**: `int?` (nullable)
- **Default**: `null` (random seed)
- **Description**: Seed for random number generator
- **Usage**: Set for reproducible simulations

### Example Configuration

```csharp
var config = new SimulationConfiguration
{
    MaxSteps = 100,
    StabilizationThreshold = 0.01,
    CheckStabilization = true,
    FeedbackSmoothingFactor = 0.3,
    RandomSeed = 42  // For reproducibility
};
```

---

## State Management

### SimulationState

Tracks the current state of the simulation:

#### Properties

- **CurrentStep**: Current simulation step number (starts at 0)
- **LastStepMigrations**: Number of migrations in the last step
- **TotalMigrations**: Cumulative migrations across all steps
- **IsStabilized**: Whether the simulation has stabilized
- **IsCompleted**: Whether the simulation has finished
- **Random**: The random number generator for this simulation

#### State Transitions

```
[Start]
   ↓
[Running] ─→ CurrentStep++, TotalMigrations updated each step
   ↓
   ├─→ [Stabilized] ─→ IsStabilized = true, IsCompleted = true
   └─→ [MaxSteps]   ─→ IsCompleted = true
```

---

## Observer Pattern

### ISimulationObserver Interface

Allows external components to monitor simulation progress without coupling to the engine.

#### Methods

```csharp
void OnSimulationStarted(SimulationState state)
```

- Called once at the beginning
- Receives initial state

```csharp
void OnStepCompleted(SimulationState state, IReadOnlyList<MigrationFlow> flows)
```

- Called after each step
- Receives updated state and migration flows from that step

```csharp
void OnSimulationCompleted(SimulationState state)
```

- Called once at the end
- Receives final state

### Built-in Observer: ConsoleSimulationObserver

A simple console logger that outputs simulation progress.

**Constructor Parameters:**

- `verbose` (bool): If true, shows detailed migration flows

**Example Output:**

```
=== Simulation Started ===
Initial Step: 0

Step 1 completed:
  Migrations this step: 245
  Total migrations: 245
  Migration details:
    CityA -> CityB: 100 people (Young Professionals)
    CityA -> CityC: 145 people (Young Professionals)

Step 2 completed:
  Migrations this step: 198
  Total migrations: 443

...

=== Simulation Completed ===
Final Step: 47
Total Migrations: 8432
Stabilized: true
```

### Custom Observers

You can implement custom observers for:

- Data collection and analysis
- Visualization updates
- Logging to files or databases
- Real-time monitoring dashboards
- Triggering events based on thresholds

**Example:**

```csharp
public class DataCollectorObserver : ISimulationObserver
{
    private List<StepData> _data = new();
    
    public void OnStepCompleted(SimulationState state, IReadOnlyList<MigrationFlow> flows)
    {
        _data.Add(new StepData
        {
            Step = state.CurrentStep,
            Migrations = state.LastStepMigrations,
            FlowCount = flows.Count
        });
    }
    
    // Other methods...
}
```

---

## Stabilization Detection

### Algorithm

The simulation can automatically detect when the system has stabilized (reached equilibrium).

```csharp
bool CheckStabilization()
{
    if (CurrentStep < 2)
        return false  // Need at least 2 steps
    
    totalPopulation = world.Population
    if (totalPopulation == 0)
        return true  // Empty world is stable
    
    migrationRate = LastStepMigrations / totalPopulation
    
    return migrationRate < StabilizationThreshold
}
```

### Interpretation

- **Migration Rate**: Fraction of total population that migrated in the last step
- **Threshold**: Typically 0.01 (1%)
- **Stable**: When migration drops below threshold, the system is in equilibrium

### Why It Matters

Stabilization detection allows simulations to:

1. **End early** when equilibrium is reached (save computation)
2. **Identify convergence** to steady state
3. **Study equilibrium** properties of the system

---

## Events

The engine provides C# events for integration:

### StepCompleted Event

```csharp
event EventHandler<SimulationStepEventArgs> StepCompleted
```

**EventArgs Properties:**

- `State`: Current simulation state
- `MigrationFlows`: Flows from this step

### SimulationCompleted Event

```csharp
event EventHandler<SimulationCompletedEventArgs> SimulationCompleted
```

**EventArgs Properties:**

- `FinalState`: Final simulation state

---

## Usage Examples

### Basic Simulation with Builder

```csharp
// 1. Create world
var world = new World(cities, factorDefinitions);

// 2. Configure and build engine
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(new SimulationConfiguration { MaxSteps = 100 })
    .UseEnhancedAttractionCalculator()
    .UseEnhancedMigrationCalculator()
    .UseEnhancedFeedbackCalculator()
    .AddConsoleObserver()
    .Build();

// 3. Run
engine.Run();
```

### Advanced Configuration

```csharp
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(config)
    .UseEnhancedMigrationCalculator(
        sigmoidSteepness: 1.5,
        costSensitivity: 0.02,
        baseMigrationCost: 2.0)
    .UseEnhancedFeedbackCalculator([
        new FactorFeedbackRule 
        { 
            Factor = housingFactor, 
            FeedbackType = FeedbackType.PriceCost,
            Elasticity = 0.5
        }
    ])
    .Build();
```

### Step-by-Step Simulation

```csharp
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(config)
    .UseEnhancedAttractionCalculator()
    .UseEnhancedMigrationCalculator()
    .UseEnhancedFeedbackCalculator()
    .Build();

while (!engine.State.IsCompleted && engine.State.CurrentStep < 100)
{
    engine.Step();
    
    // Custom logic after each step
    AnalyzeResults(engine.State);
    
    if (ShouldStopEarly())
        break;
}
```

### Custom Pipeline

```csharp
// Create custom pipeline
var pipeline = new SimulationPipeline();
pipeline.AddStage(new AttractionStage(attractionCalc));
pipeline.AddStage(new CustomAnalysisStage()); // Your custom stage
pipeline.AddStage(new MigrationStage(migrationCalc));
pipeline.AddStage(new MigrationApplicationStage());
pipeline.AddStage(new FeedbackStage(feedbackCalc));

var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(config)
    .WithCustomPipeline(pipeline)
    .Build();
```

### With Event Handlers

```csharp
var engine = new SimulationEngineBuilder()
    .WithWorld(world)
    .WithConfiguration(config)
    .UseEnhancedAttractionCalculator()
    .UseEnhancedMigrationCalculator()
    .UseEnhancedFeedbackCalculator()
    .Build();

engine.StepCompleted += (sender, args) =>
{
    Console.WriteLine($"Step {args.State.CurrentStep}: {args.MigrationFlows.Count} flows");
};

engine.SimulationCompleted += (sender, args) =>
{
    Console.WriteLine($"Completed at step {args.FinalState.CurrentStep}");
};

engine.Run();
```

---

## Performance Considerations

### Computational Complexity

Per step, for N cities and P population groups:

- **Attraction Calculation**: O(N × P × F) where F is average factors per city
- **Migration Calculation**: O(N × P × N) = O(N² × P)
- **Feedback Application**: O(N)

**Overall**: O(N² × P × F) per step

### Optimization Strategies

1. **Early Stabilization**: Enable `CheckStabilization` to end when equilibrium is reached
2. **Factor Caching**: Normalized factor values could be cached if factors don't change
3. **Spatial Indexing**: For large numbers of cities, consider spatial data structures
4. **Parallel Processing**: City-group pairs can be processed independently (future enhancement)

---

## Design Patterns Used

1. **Pipeline Pattern**: Modular, configurable sequence of processing stages
2. **Observer Pattern**: For simulation monitoring
3. **Strategy Pattern**: Calculators can be swapped via interfaces
4. **Builder Pattern**: Fluent API for engine configuration
5. **Dependency Injection**: World, configuration, and calculators injected via constructor

---

## Extension Points

The simulation engine can be extended through:

1. **Custom Pipeline Stages**: Implement `ISimulationStage`
2. **Custom Calculators**: Implement `IAttractionCalculator`, `IMigrationCalculator`, or `IFeedbackCalculator`
3. **Custom Observers**: Implement `ISimulationObserver`
4. **Custom Configuration**: Extend `SimulationConfiguration`
5. **Event Handlers**: Subscribe to `StepCompleted` and `SimulationCompleted` events

### Creating Custom Pipeline Stages

```csharp
public class CustomAnalysisStage : ISimulationStage
{
    public string Name => "Custom Analysis";
    public int Order => 150; // Between Migration and Application stages
    
    public SimulationStageResult Execute(SimulationContext context)
    {
        // Your custom logic here
        var attractions = context.SharedData["Attractions"] as List<AttractionResult>;

        // Analyze and potentially modify data
        PerformAnalysis(attractions);

        return SimulationStageResult.Successful("Analysis completed");
    }
}
```