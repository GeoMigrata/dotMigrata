# Simulation Engine Documentation

This directory contains the simulation engine that orchestrates the population migration simulation.

## Overview

The Simulation layer provides the orchestration framework that runs the step-by-step simulation, managing state,
configuration, and observation of the simulation process.

## Components

### 1. SimulationEngine

The main orchestrator that coordinates all simulation activities.

### 2. SimulationConfiguration

Configuration parameters that control simulation behavior.

### 3. SimulationState

Tracks the current state and statistics of a running simulation.

### 4. ISimulationObserver

Observer interface for monitoring simulation progress.

---

## SimulationEngine Design

### Architecture

The `SimulationEngine` follows the **Observer Pattern** and coordinates three main calculators:

- `AttractionCalculator` - Computes city attractiveness
- `MigrationCalculator` - Determines migration flows
- `FeedbackCalculator` - Updates city factors

### Key Responsibilities

1. **Orchestration**: Coordinates the execution of attraction, migration, and feedback calculations
2. **State Management**: Maintains and updates simulation state
3. **Observer Notification**: Notifies observers of simulation events
4. **Stabilization Detection**: Monitors when the system reaches equilibrium

---

## Simulation Flow

### Initialization

```
1. Create World with cities, factors, and population groups
2. Create SimulationConfiguration with parameters
3. Create SimulationEngine(world, configuration)
4. Add observers (optional)
5. Call engine.Run() or engine.Step()
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

Each call to `Step()` performs one complete simulation cycle:

```
┌─────────────────────────────────────────────────┐
│ Step N                                          │
├─────────────────────────────────────────────────┤
│                                                 │
│  1. For each city and population group:        │
│     ┌─────────────────────────────────────┐    │
│     │ Calculate Attractions               │    │
│     │  - AttractionCalculator             │    │
│     │  - Produces AttractionResults       │    │
│     └─────────────────────────────────────┘    │
│              ↓                                  │
│     ┌─────────────────────────────────────┐    │
│     │ Calculate Migration Flows           │    │
│     │  - MigrationCalculator              │    │
│     │  - Produces MigrationFlows          │    │
│     └─────────────────────────────────────┘    │
│                                                 │
│  2. Apply all migration flows:                 │
│     ┌─────────────────────────────────────┐    │
│     │ Update Population Counts            │    │
│     │  - Move people between cities       │    │
│     │  - Track migration statistics       │    │
│     └─────────────────────────────────────┘    │
│              ↓                                  │
│  3. Apply feedback effects:                    │
│     ┌─────────────────────────────────────┐    │
│     │ Update City Factors                 │    │
│     │  - FeedbackCalculator               │    │
│     │  - Adjust factors based on pop.     │    │
│     └─────────────────────────────────────┘    │
│              ↓                                  │
│  4. Update state and notify:                   │
│     ┌─────────────────────────────────────┐    │
│     │ Advance Step Counter                │    │
│     │ Record Statistics                   │    │
│     │ Notify Observers                    │    │
│     └─────────────────────────────────────┘    │
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
    foreach (city in world.Cities)
    {
        foreach (group in city.PopulationGroups)
        {
            // Calculate how attractive each city is to this group
            attractions = attractionCalculator
                .CalculateAttractionForAllCities(world, group)
            
            // Determine where this group wants to migrate
            flows = migrationCalculator
                .CalculateMigrationFlows(city, group, attractions, world, random)
            
            allMigrationFlows.AddRange(flows)
        }
    }
    
    // Phase 2: Apply migrations
    totalMigrants = ApplyMigrations(allMigrationFlows)
    
    // Phase 3: Apply feedback
    foreach (city in world.Cities)
    {
        feedbackCalculator.ApplyFeedback(city, previousPop, currentPop)
    }
    
    // Phase 4: Update state
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

### Basic Simulation

```csharp
// 1. Create world
var world = new World(cities, factorDefinitions);

// 2. Configure
var config = new SimulationConfiguration { MaxSteps = 100 };

// 3. Create engine
var engine = new SimulationEngine(world, config);

// 4. Add observer
engine.AddObserver(new ConsoleSimulationObserver(verbose: true));

// 5. Run
engine.Run();
```

### Step-by-Step Simulation

```csharp
var engine = new SimulationEngine(world, config);

while (!engine.State.IsCompleted && engine.State.CurrentStep < 100)
{
    engine.Step();
    
    // Custom logic after each step
    AnalyzeResults(engine.State);
    
    if (ShouldStopEarly())
        break;
}
```

### With Event Handlers

```csharp
var engine = new SimulationEngine(world, config);

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

1. **Observer Pattern**: For simulation monitoring
2. **Strategy Pattern**: Calculators can be swapped (though currently concrete)
3. **Template Method**: `Run()` defines the simulation skeleton
4. **Dependency Injection**: World and configuration injected via constructor

---

## Extension Points

The simulation engine can be extended through:

1. **Custom Observers**: Implement `ISimulationObserver`
2. **Custom Configuration**: Extend `SimulationConfiguration`
3. **Event Handlers**: Subscribe to `StepCompleted` and `SimulationCompleted` events
4. **Custom State Tracking**: Extend `SimulationState` (though internal methods limit this)

For algorithm customization, extend or replace the calculators in the Logic layer.