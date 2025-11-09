# dotGeoMigrata

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache2.0-blue.svg)](LICENSE)

dotGeoMigrata is a C# .NET 9.0 simulation framework designed to model population migration and city evolution in a
multi-city, multi-population system. The framework captures how city characteristics influence population movement and
how migration feedback affects city factors over time.

## Core Idea

The main simulation loop follows:

```text
City Factors -> Population Preferences -> Attraction Differences -> Partial Migration -> City Feedback -> Iterative Evolution
```

Population migration is driven by city factors, and in turn, migration reshapes city factors in an ongoing iterative
process.

## Key Concepts

- **World:** Top-level entity containing cities, factor definitions, and population group definitions. Maintains global
  factor definitions, population group definitions, and current city states.
- **City:** Contains factor values (e.g., income, pollution, public services) and population group values for each
  defined population group. Each city has values for all factor definitions and all population group definitions defined
  in the world.
- **FactorDefinition & FactorValue:** Define metadata for each factor, including direction (pull or push),
  normalization, and value range. Factor values are normalized for internal calculations. Each city has a FactorValue
  for every FactorDefinition in the world.
- **PopulationGroupDefinition & PopulationGroupValue:** Define population groups with shared migration behavior.
  PopulationGroupDefinition specifies move willingness, retention rate, and sensitivities to various factors.
  PopulationGroupValue represents the actual population count of that group in a specific city. Each city has a
  PopulationGroupValue for every PopulationGroupDefinition in the world (count can be 0). This design allows reusing
  the same population group characteristics across multiple cities without duplication.
- **Attraction:** Computes net attractiveness of a city for a population group considering normalized factor values,
  group sensitivities, and factor directions.
- **Migration:** Migration decisions are based on attraction differences, thresholds, and migration cost. Migration
  is modeled via probabilistic sampling considering group size and city capacity. Instead of adding/removing population
  groups, migrations update the population counts in PopulationGroupValues.
- **City Feedback:** After migration, city factors are updated according to feedback mechanisms (per capita resources,
  housing costs, congestion/pollution, industrial/economic effects), typically with smoothing to avoid abrupt changes.

## Simulation Flow

1. Initialize the world with cities, factor definitions, population group definitions, and sensitivities.
    - Each PopulationGroupDefinition must have sensitivities for all FactorDefinitions
    - Each City must have FactorValues for all FactorDefinitions
    - Each City must have PopulationGroupValues for all PopulationGroupDefinitions
2. For each simulation step:
    - Normalize city factors.
    - For each population group definition, calculate city attraction across all cities.
    - Compute migration probability for each group toward possible destination cities.
    - Sample actual migration numbers considering capacity and retention.
    - Update city population composition by modifying PopulationGroupValue counts.
    - Update city factors based on migration feedback.
3. Repeat until simulation ends (max time steps or system stabilizes).

## Installation & Usage

### Adding to Your Project

Add the library to your .NET 9.0 project:

```bash
dotnet add reference /path/to/dotGeoMigrata.csproj
# Or, once published to NuGet:
# dotnet add package dotGeoMigrata
```

### Quick Start Example

Here's a simple example using the fluent builder API:

```csharp
using dotGeoMigrata;
using dotGeoMigrata.Core.Enums;

// Create a world with factors, population groups, and cities
var world = new WorldBuilder()
    .WithName("Example World")
    // Define factors that influence migration
    .AddFactor("Income", FactorType.Positive, 20000, 100000)
    .AddFactor("Pollution", FactorType.Negative, 0, 100)
    .AddFactor("Housing Cost", FactorType.Negative, 500, 3000)
    // Define population groups with migration behaviors
    .AddPopulationGroup("Young Professionals", 
        movingWillingness: 0.7, 
        retentionRate: 0.3,
        group => group
            .WithSensitivity("Income", 5)
            .WithSensitivity("Pollution", -2)
            .WithSensitivity("Housing Cost", -3))
    // Add cities with initial conditions
    .AddCity("City A", 
        latitude: 26.0, longitude: 119.3, area: 100.0, capacity: 1000000,
        city => city
            .WithFactorValue("Income", 50000)
            .WithFactorValue("Pollution", 30)
            .WithFactorValue("Housing Cost", 1500)
            .WithPopulation("Young Professionals", 100000))
    .AddCity("City B",
        latitude: 24.5, longitude: 118.1, area: 80.0, capacity: 800000,
        city => city
            .WithFactorValue("Income", 40000)
            .WithFactorValue("Pollution", 20)
            .WithFactorValue("Housing Cost", 1000)
            .WithPopulation("Young Professionals", 80000))
    .Build();

// Create and run simulation
var result = await new SimulationBuilder()
    .WithWorld(world)
    .UseStandardPipeline()
    .AddConsoleObserver(colored: true)
    .BuildAndRunAsync();

Console.WriteLine($"Simulation completed after {result.CurrentTick} ticks");
```

### Advanced Usage

For more control, you can configure individual components:

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
    StabilityThreshold = 10,
    StabilityCheckInterval = 1,
    MinTicksBeforeStabilityCheck = 10
};

// Build with custom configuration
var engine = new SimulationBuilder()
    .WithWorld(world)
    .WithModelConfig(modelConfig)
    .WithSimulationConfig(simConfig)
    .UseStandardPipeline()
    .AddConsoleObserver()
    .Build();

var context = await engine.RunAsync(world);
```

### Using Snapshots

Save and restore simulation states:

```csharp
using dotGeoMigrata.Snapshot.Services;
using dotGeoMigrata.Snapshot.Serialization;

// Create snapshot after simulation
var snapshot = SnapshotService.CreateSnapshot(world);

// Save to JSON
var jsonSerializer = new JsonSnapshotSerializer();
await jsonSerializer.SerializeAsync(snapshot, "simulation-output.json");

// Restore from snapshot
var loadedSnapshot = await jsonSerializer.DeserializeAsync("simulation-output.json");
var restoredWorld = SnapshotService.RestoreWorld(loadedSnapshot);
```

## Architecture

### Core Layer (`/src/Core`)

Contains fundamental domain models:

- `World`, `City` - Entity models
- `FactorDefinition`, `GroupDefinition` - Definition models
- `FactorValue`, `GroupValue` - Value models

### Logic Layer (`/src/Logic`)

Provides calculation interfaces and implementations:

- `IAttractionCalculator` - Calculates city attraction for groups
- `IMigrationCalculator` - Determines migration flows
- `StandardAttractionCalculator` / `StandardMigrationCalculator` - Default implementation based on scientific models

### Simulation Layer (`/src/Simulation`)

Implements a pipeline-based simulation engine:

- `ISimulationStage` - Extensible stage interface
- `SimulationEngine` - Tick-based orchestrator
- Built-in stages: `AttractionCalculationStage`, `MigrationDecisionStage`, `MigrationExecutionStage`
- `ISimulationObserver` - Observer pattern for monitoring (includes `ConsoleObserver`)

### Snapshot Layer (`/src/Snapshot`)

Git-like incremental snapshot system:

- Stores initial world state + simulation steps (deltas)
- Supports JSON and XML serialization
- Efficient storage with migration event records

## Public API

### Main Entry Points

The library provides fluent builders for ease of use:

- **`WorldBuilder`** - Construct worlds with cities, factors, and population groups
- **`SimulationBuilder`** - Configure and create simulation engines

### Core Abstractions

Extend the framework by implementing these interfaces:

- **`IAttractionCalculator`** - Custom logic for calculating city attractiveness
- **`IMigrationCalculator`** - Custom logic for determining migration flows
- **`ISimulationStage`** - Custom stages to add to the simulation pipeline
- **`ISimulationObserver`** - Monitor and react to simulation events

### Key Models

Domain models available for use and extension:

- **`World`**, **`City`** - Core simulation entities
- **`FactorDefinition`**, **`FactorValue`** - City characteristic system
- **`GroupDefinition`**, **`GroupValue`** - Population group system
- **`SimulationContext`** - Runtime simulation state
- **`AttractionResult`**, **`MigrationFlow`** - Calculation results

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
2. **State Management**: Use `SnapshotService` to save/restore simulation state
3. **Custom Stages**: Inject logging, metrics, or custom logic via `ISimulationStage`
4. **Serialization**: JSON/XML snapshots are ready for API responses

### Example Observer for API Integration

```csharp
public class ApiStreamingObserver : ISimulationObserver
{
    private readonly IHubContext<SimulationHub> _hubContext;

    public void OnTickComplete(SimulationContext context)
    {
        // Stream tick updates to connected clients
        _hubContext.Clients.All.SendAsync("TickUpdate", new
        {
            Tick = context.CurrentTick,
            PopulationChange = context.TotalPopulationChange,
            Cities = context.World.Cities.Select(c => new
            {
                c.DisplayName,
                c.Population
            })
        });
    }
    // ... other methods
}
```

## Contributing

Contributions, bug reports, and feature requests are welcome. Please submit via GitHub issues or pull requests.