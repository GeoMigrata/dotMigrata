# dotGeoMigrata

[![.NET8.0](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![.NET9.0](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache2.0-blue.svg)](LICENSE)

dotGeoMigrata is a C# .NET 9.0 simulation framework designed to model individual-based population migration and city
evolution in a multi-city system. The framework simulates individual persons (10,000 to 1,000,000+) with unique
characteristics, capturing how city factors influence individual migration decisions and how migration feedback affects
city dynamics over time.

## Core Idea

The main simulation loop follows:

```text
City Factors -> Individual Preferences -> Attraction Differences -> Individual Migration Decisions -> City Feedback -> Iterative Evolution
```

Individual migration decisions are driven by city factors and personal preferences, and in turn, migration reshapes city
factors in an ongoing iterative process.

## Key Concepts

- **World:** Top-level entity containing cities and factor definitions. Maintains global factor definitions and current
  city states with their populations.
- **City:** Contains factor values (e.g., income, pollution, public services) and a collection of individual persons
  residing in the city. Each city has values for all factor definitions defined in the world.
- **Person:** Individual entity with unique ID, personalized factor sensitivities, migration willingness, and retention
  rate. Each person makes independent migration decisions based on their own preferences.
- **FactorDefinition & FactorValue:** Define metadata for each factor, including direction (pull or push),
  normalization, and value range. Factor values are normalized for internal calculations. Each city has a FactorValue
  for every FactorDefinition in the world.
- **PersonGenerator:** Module for generating large populations (10,000 to 1,000,000+) with randomized attributes using
  configurable distributions (normal distribution for sensitivities).
- **Attraction:** Computes net attractiveness of a city for an individual person considering normalized factor values,
  personal sensitivities, and factor directions.
- **Migration:** Individual migration decisions are based on attraction differences, personal thresholds, and
  willingness to move. Each person independently decides whether to migrate and to which city, considering distance,
  capacity, and personal preferences.
- **City Feedback:** After migration, city factors can be updated according to feedback mechanisms (per capita
  resources, housing costs, congestion/pollution, industrial/economic effects), typically with smoothing to avoid abrupt
  changes.

## Simulation Flow

1. Initialize the world with cities, factor definitions, and persons.
    - Each City must have FactorValues for all FactorDefinitions
    - Persons are generated with randomized sensitivities and attributes
    - Each Person is assigned to an initial city
2. For each simulation step:
    - For each person, calculate attraction to all cities based on personal sensitivities
    - Each person independently decides whether to migrate based on attraction differences and personal willingness
    - Execute migrations by moving persons between cities (thread-safe operations)
    - Optionally update city factors based on migration feedback
3. Repeat until simulation ends (max time steps or system stabilizes).

## Installation & Usage

### Adding to Your Project

Add the library to your .NET 9.0 project:

```bash
dotnet add reference /path/to/dotGeoMigrata.csproj
# Or, once published to NuGet:
# dotnet add package GeoMigrata.Framework
```

### Quick Start Example

Here's a simple example using the fluent builder API:

```csharp
using dotGeoMigrata.Builder;
using dotGeoMigrata.Core.Enums;

// Create a world with factors and cities populated with individual persons
var world = new WorldBuilder()
    .WithName("Example World")
    
    // Define factors that influence individual migration decisions
    .AddFactor("Income", FactorType.Positive, 20000, 100000)
    .AddFactor("Pollution", FactorType.Negative, 0, 100)
    .AddFactor("Housing Cost", FactorType.Negative, 500, 3000)
    
    // Add cities with initial populations of individuals
    .AddCity("City A", 
        latitude: 26.0, longitude: 119.3, area: 100.0, capacity: 1000000,
        city => city
            .WithFactorValue("Income", 50000)
            .WithFactorValue("Pollution", 30)
            .WithFactorValue("Housing Cost", 1500)
            .WithRandomPersons(100000))  // Generate 100,000 individual persons
    
    .AddCity("City B",
        latitude: 24.5, longitude: 118.1, area: 80.0, capacity: 800000,
        city => city
            .WithFactorValue("Income", 40000)
            .WithFactorValue("Pollution", 20)
            .WithFactorValue("Housing Cost", 1000)
            .WithRandomPersons(80000))  // Generate 80,000 individual persons
    
    .Build();

// Create and run simulation
var result = await new SimulationBuilder()
    .WithWorld(world)
    .UseStandardPipeline()
    .AddConsoleObserver(colored: true)
    .BuildAndRunAsync();

Console.WriteLine($"Simulation completed after {result.CurrentTick} ticks");
Console.WriteLine($"Final population: {result.World.Population:N0} persons");
```

### Advanced Usage - Custom Person Generation

For more control over person attributes, you can configure the person generator:

```csharp
using dotGeoMigrata.Generator;

// Configure person generation with custom parameters
var personConfig = new PersonGeneratorConfig
{
    MinMovingWillingness = 0.1,
    MaxMovingWillingness = 0.9,
    MinRetentionRate = 0.1,
    MaxRetentionRate = 0.9,
    MinSensitivity = -10.0,
    MaxSensitivity = 10.0,
    SensitivityStdDev = 3.0,  // Standard deviation for normal distribution
    RandomSeed = 42  // For reproducible results
};

// Use custom configuration when building cities
var world = new WorldBuilder()
    .WithName("Custom World")
    .AddFactor("Income", FactorType.Positive, 30000, 150000)
    .AddCity("City A", 26.0, 119.3, 100.0, capacity: 500000,
        city => city
            .WithFactorValue("Income", 80000)
            .WithRandomPersons(50000, personConfig))  // Use custom config
    .Build();
```

### Configuring Simulation Parameters

You can also configure the simulation execution and model parameters:

```csharp
using dotGeoMigrata.Logic.Models;
using dotGeoMigrata.Simulation.Models;

// Configure model parameters
var modelConfig = new StandardModelConfig
{
    CapacitySteepness = 5.0,
    DistanceDecayLambda = 0.001,
    MigrationProbabilitySteepness = 10.0,
    MigrationProbabilityThreshold = 0.0,
    FactorSmoothingAlpha = 0.2
};

// Configure simulation parameters
var simConfig = new SimulationConfig
{
    MaxTicks = 500,
    CheckStability = true,
    StabilityThreshold = 100,  // Consider stable if <100 persons migrate
    StabilityCheckInterval = 5,
    MinTicksBeforeStabilityCheck = 20
};

// Build with custom configuration
var engine = new SimulationBuilder()
    .WithWorld(world)
    .WithModelConfig(modelConfig)
    .WithSimulationConfig(simConfig)
    .UseStandardPipeline()
    .AddConsoleObserver(colored: true)
    .Build();

var context = await engine.RunAsync(world);
```

## Architecture

### Core Layer (`/src/Core`)

Contains fundamental domain models:

- `World`, `City` - Entity models
- `Person` - Individual entity with unique attributes
- `FactorDefinition`, `FactorValue` - Factor system models
- `Coordinate` - Geographic coordinate model

### Logic Layer (`/src/Logic`)

Provides calculation interfaces and implementations:

- `IAttractionCalculator` - Calculates city attraction for individual persons
- `IMigrationCalculator` - Determines individual migration decisions
- `StandardAttractionCalculator` / `StandardMigrationCalculator` - Default implementations with parallel processing

### Simulation Layer (`/src/Simulation`)

Implements a pipeline-based simulation engine:

- `ISimulationStage` - Extensible stage interface
- `SimulationEngine` - Tick-based orchestrator
- Built-in stages: `MigrationDecisionStage`, `MigrationExecutionStage`
- `ISimulationObserver` - Observer pattern for monitoring (includes `ConsoleObserver`)

### Generator Layer (`/src/Generator`)

Person generation module:

- `PersonGenerator` - Generates large populations with randomized attributes
- `PersonGeneratorConfig` - Configuration for person generation (distributions, ranges, seeds)

### Snapshot Layer (`/src/Snapshot`)

Snapshot system for saving simulation states:

- `SnapshotService` - Create and manage snapshots (stub implementation)
- JSON serialization support
- *Note: Full person-based snapshot restoration is pending implementation*

## Performance Characteristics

### Scalability

The framework uses parallel processing (PLINQ) to efficiently handle large populations:

- **Small**: 10,000 - 50,000 persons (~3-15 MB, <1-3 seconds per tick)
- **Medium**: 50,000 - 200,000 persons (~15-60 MB, 3-10 seconds per tick)
- **Large**: 200,000 - 1,000,000 persons (~60-300 MB, 10-90 seconds per tick)

*Performance varies based on CPU cores, factor count, and city count*

### Memory Efficiency

- Per-person memory: ~300 bytes (100 base + 200 for sensitivities)
- Thread-safe concurrent operations using ConcurrentDictionary
- Efficient factor value lookups

## Public API

### Main Entry Points

The library provides fluent builders for ease of use:

- **`WorldBuilder`** - Construct worlds with cities, factors, and populations
- **`SimulationBuilder`** - Configure and create simulation engines

### Core Abstractions

Extend the framework by implementing these interfaces:

- **`IAttractionCalculator`** - Custom logic for calculating city attractiveness
- **`IMigrationCalculator`** - Custom logic for determining individual migration decisions
- **`ISimulationStage`** - Custom stages to add to the simulation pipeline
- **`ISimulationObserver`** - Monitor and react to simulation events

### Key Models

Domain models available for use and extension:

- **`World`**, **`City`**, **`Person`** - Core simulation entities
- **`FactorDefinition`**, **`FactorValue`** - City characteristic system
- **`PersonGenerator`**, **`PersonGeneratorConfig`** - Population generation
- **`SimulationContext`** - Runtime simulation state
- **`AttractionResult`**, **`MigrationFlow`** - Calculation results

## Examples

See the `/examples` directory for complete working examples:

- **`PersonBasedExample.cs`** - Fujian Province simulation with 180,000 persons across 5 cities
- **`README.md`** - Detailed explanation of features and usage

## Migration from PopulationGroup Architecture

If you're migrating from the old PopulationGroup-based architecture, see:

- **`MIGRATION_GUIDE.md`** - Complete API migration guide
- **`REFACTORING_SUMMARY.md`** - Technical details of the refactoring

### Key Changes

- **Removed**: `GroupDefinition`, `GroupValue` classes
- **New**: `Person` entity with individual attributes
- **API**: `WorldBuilder` now uses `.WithRandomPersons()` instead of `.AddPopulationGroup()`
- **Simulation**: Individual decision-making instead of group-level aggregation

## Extensibility for REST API / Middleware

The library is designed to be consumed by middleware layers (console apps, web APIs, etc.). Key considerations:

### Recommended Architecture

```
┌─────────────────────────────────┐
│   Visualization / Client App    │
│   (React, Vue, Desktop, etc.)   │
└────────────┬────────────────────┘
             │ HTTP/WebSocket
┌────────────▼────────────────────┐
│   Console/Web Middleware API    │
│   (ASP.NET Core, REST, gRPC)    │
│   - Exposes simulation controls │
│   - Streams simulation updates  │
│   - Manages state persistence   │
└────────────┬────────────────────┘
             │ Direct Reference
┌────────────▼────────────────────┐
│    dotGeoMigrata Library        │
│    (This Package)               │
└─────────────────────────────────┘
```

### Integration Points

1. **Real-time Updates**: Use `ISimulationObserver` to stream events via SignalR/WebSocket
2. **State Management**: Use `SnapshotService` for basic snapshot creation
3. **Custom Stages**: Inject logging, metrics, or custom logic via `ISimulationStage`
4. **Serialization**: JSON snapshots are ready for API responses

## Contributing

Contributions, bug reports, and feature requests are welcome. Please submit via GitHub issues or pull requests.

## License

Apache 2.0 - See LICENSE file for details.