﻿# dotMigrata

[![.NET8.0](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![.NET9.0](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![.NET10.0](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache2.0-blue.svg)](LICENSE)
[![Release](https://github.com/GeoMigrata/dotMigrata/actions/workflows/release.yml/badge.svg)](https://github.com/GeoMigrata/dotMigrata/actions/workflows/release.yml)

dotMigrata, a product from Project GeoMigrata, is a C# .NET simulation framework designed to model individual-based
population migration and city
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
- **PersonBase/StandardPerson:** Base class and standard implementation for individual entities with unique
  characteristics. `PersonBase` is an abstract class defining essential migration properties, while `StandardPerson` is
  the concrete implementation with additional behavioral properties. Each person makes independent migration decisions
  based on their own preferences.
- **FactorDefinition & FactorIntensity:** Define metadata for each factor, including direction (pull or push),
  normalization, and value range. Factor intensities are stored as ValueSpec and evaluated lazily for calculations. Each
  city has a FactorIntensity
  for every FactorDefinition in the world.
- **PersonGenerator:** Module for generating large populations (10,000 to 1,000,000+) with randomized attributes using
  configurable distributions (normal distribution for sensitivities).
- **Events:** Fundamental simulation mechanism that modifies city factor values during runtime. Events support various
  triggers (one-time, periodic, continuous, conditional) and effect types (absolute, delta, multiply, linear/logarithmic
  transitions). Enables dynamic scenarios like policy changes, disasters, or economic shifts.
- **Attraction:** Computes net attractiveness of a city for an individual person considering normalized factor values,
  personal sensitivities, and factor directions.
- **Migration:** Individual migration decisions are based on attraction differences, personal thresholds, and
  willingness to move. Each person independently decides whether to migrate and to which city, considering distance,
  capacity, and personal preferences.
- **Feedback:** Feedback strategies (e.g., congestion, per-capita resources) are integrated through the event system,
  running as periodic events that dynamically adjust city factors based on population changes.

## Simulation Flow

1. Initialize the world with cities, factor definitions, and persons.
    - Each City must have FactorIntensities for all FactorDefinitions
    - Persons are generated with randomized sensitivities and attributes
    - Each person (PersonBase/StandardPerson instance) is assigned to an initial city
2. For each simulation step:
    - For each person, calculate attraction to all cities based on personal sensitivities
    - Each person independently decides whether to migrate based on attraction differences and personal willingness
    - Execute migrations by moving persons between cities (thread-safe operations)
   - Execute events based on their triggers (one-time, periodic, continuous, or conditional)
   - Events modify city factors dynamically (absolute changes, transitions, feedback mechanisms)
3. Repeat until simulation ends (max time steps or system stabilizes).

## Key Features

### Person-Based Simulation

- **Individual-level modeling** with 10,000 to 1,000,000+ persons
- **Unique characteristics** for each person (sensitivities, willingness, retention)
- **Independent decision-making** based on personal preferences
- **Tag-based categorization** for statistical analysis

### PersonCollection System

- **Flexible population generation** with Individual, Individuals (duplicates), and Generator specifications
- **Reproducible simulations** using random seeds
- **Mixed populations** combining exact individuals with procedurally generated cohorts
- **Efficient serialization** storing specifications instead of individual instances

### Parallel Processing

- **PLINQ-based** computation for scalable performance
- **Thread-safe operations** using concurrent collections
- **Multi-core optimization** for large-scale simulations

### Extensibility

- **Pipeline-based architecture** with custom simulation stages
- **Observer pattern** for real-time monitoring
- **Custom calculators** for attraction and migration logic
- **Pluggable algorithms** for different simulation models

### State Management

- **XML snapshot system** with deterministic reproducibility
- **Step-based tracking** for simulation replay
- **Namespace-based format** distinguishing code concepts from containers
- **Compact storage** supporting millions of persons

## Architecture

### Core Layer (`/src/Core`)

Contains fundamental domain models:

- `World`, `City` - Entity models
- `PersonBase` - Abstract base class for all person types
- `StandardPerson` - Standard implementation with migration-specific properties
- `FactorDefinition`, `FactorIntensity` - Factor system models
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
- Built-in stages: `MigrationDecisionStage`, `MigrationExecutionStage`, `EventStage`
- `ISimulationObserver` - Observer pattern for monitoring (includes `ConsoleObserver`)

#### Events Subsystem (`/src/Simulation/Events`)

Core event system for dynamic factor modifications:

- `ISimulationEvent`, `SimulationEvent` (`dotMigrata.Simulation.Events`, `dotMigrata.Simulation.Events.Interfaces`) -
  Event abstractions and implementations
- `IEventTrigger` (`dotMigrata.Simulation.Events.Interfaces`) - Trigger interface with multiple implementations under
  `dotMigrata.Simulation.Events.Triggers`:
    - `TickTrigger` - One-time execution at specific tick
    - `PeriodicTrigger` - Repeated execution at intervals
    - `ContinuousTrigger` - Continuous execution within time window
    - `ConditionalTrigger` - Condition-based execution (extension point for ECA patterns)
- `IEventEffect` (`dotMigrata.Simulation.Events.Interfaces`) - Effect interface with implementations under
  `dotMigrata.Simulation.Events.Effects`:
    - `FactorChangeEffect` - Modify city factor values (absolute, delta, multiply, transitions)
    - `FeedbackEffect` - Adapter for feedback strategies
    - `CompositeEffect` - Multiple effects in sequence
- `EffectApplicationType` (`dotMigrata.Simulation.Events.Enums`) - Application types (absolute, delta, multiply,
  linear/logarithmic transitions)
- `EffectDuration` (`dotMigrata.Simulation.Events.Effects`) - Duration specification (instant or over N ticks)
- `EventStage` (`dotMigrata.Simulation.Events.EventStage`) - Pipeline stage for event execution

### Generator Layer (`/src/Generator`)

Person generation module:

- `IPersonGenerator<TPerson>` - Generic interface for custom person generators
- `StandardPersonGenerator` - Generator for StandardPerson with configurable attributes
- `PersonCollection` - Flexible population specifications with Individual, Individuals, and Generator support

### Snapshot Layer (`/src/Snapshot`)

Complete snapshot system for saving and restoring simulation states:

- **XML Serialization** - Attribute-based XML format using `System.Xml.Serialization`
- **PersonCollection Storage** - Stores collection specifications instead of individual persons
- **Deterministic Reproducibility** - Uses random seeds to regenerate exact simulation states
- **Namespace Design** - Distinguishes code concepts (`c:StandardPerson`, `c:City`) from snapshot containers
- **Efficient Format** - Compact XML supporting millions of persons

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

The framework provides the following main components:

- **`World`** - Create worlds by directly instantiating with cities and factor definitions
- **`City`** - Create cities with factor values and persons
- **`SimulationEngine`** - Create and run simulations with custom stages and configuration
- **`PersonCollection`** - Generate persons with flexible specifications

### Core Abstractions

Extend the framework by implementing these interfaces:

- **`IAttractionCalculator`** - Custom logic for calculating city attractiveness
- **`IMigrationCalculator`** - Custom logic for determining individual migration decisions
- **`ISimulationStage`** - Custom stages to add to the simulation pipeline
- **`ISimulationObserver`** - Monitor and react to simulation events

### Key Models

Domain models available for use and extension:

- **`World`**, **`City`**, **`PersonBase`**, **`StandardPerson`** - Core simulation entities
- **`FactorDefinition`**, **`FactorIntensity`** - City characteristic system
- **`IPersonGenerator<TPerson>`**, **`StandardPersonGenerator`** - Population generation
- **`SimulationContext`** - Runtime simulation state
- **`AttractionResult`**, **`MigrationFlow`** - Calculation results

## Working with Snapshots

The snapshot system enables deterministic simulation state management through XML serialization.

### Loading and Running a Simulation from Snapshot

```csharp
using dotMigrata.Snapshot.Conversion;
using dotMigrata.Snapshot.Serialization;
using dotMigrata.Simulation.Builders;

// Load snapshot from file and convert to World
var snapshot = XmlSnapshotSerializer.DeserializeFromFile("path/to/snapshot.xml");
var world = SnapshotConverter.ToWorld(snapshot!);

// Create and run simulation using fluent builder
var engine = SimulationBuilder.Create()
    .WithConsoleOutput()
    .WithRandomSeed(42)
    .ConfigureSimulation(s => s.MaxTicks(100))
    .Build();

var result = await engine.RunAsync(world);
Console.WriteLine($"Simulation completed in {result.CurrentTick} ticks");
```

### Exporting a Snapshot to File

Convert a World back to snapshot format for persistence:

```csharp
using dotMigrata.Snapshot.Conversion;
using dotMigrata.Snapshot.Serialization;
using dotMigrata.Snapshot.Enums;

// Export current simulation state to snapshot
var snapshot = SnapshotConverter.ToSnapshot(
    world, 
    SnapshotStatus.Completed, 
    currentStep: result.CurrentTick);

// Save to file
XmlSnapshotSerializer.SerializeToFile(snapshot, "output-snapshot.xml");
```

**Key Points:**

- Use `SnapshotConverter.ToWorld()` to load snapshots as runnable simulations
- Use `SnapshotConverter.ToSnapshot()` to export simulation states
- Snapshots use PersonCollection specifications rather than individual person instances
- Random seeds ensure deterministic reproducibility
- See [API.md](API.md) for detailed snapshot schema and examples
- See [examples/example-snapshot.xml](examples/example-snapshot.xml) for a complete working snapshot

## Simulation Metrics

The framework includes metrics collection for academic analysis:

```csharp
using dotMigrata.Simulation.Metrics;
using dotMigrata.Simulation.Builders;

// Create simulation with metrics collection
var metricsObserver = new MetricsObserver();
var engine = SimulationBuilder.Create()
    .WithConsoleOutput()
    .AddObserver(metricsObserver)
    .Build();

var result = await engine.RunAsync(world);

// Access collected metrics
var metrics = metricsObserver.Collector;
Console.WriteLine($"Average migration rate: {metrics.AverageMigrationRate:P2}");
Console.WriteLine($"Gini coefficient: {metrics.CurrentMetrics?.PopulationGiniCoefficient:F4}");

// Export to CSV for analysis
File.WriteAllText("simulation_metrics.csv", metrics.ExportToCsv());
```

**Available Metrics:**

- Migration rates and counts per tick
- Population distribution statistics (Gini, Entropy, CV)
- Per-city metrics (incoming/outgoing migrations, capacity utilization)
- Tag-based population analysis

## Documentation

- **[USAGE.md](USAGE.md)** - Detailed usage examples and code snippets
- **[API.md](API.md)** - Complete API reference documentation

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
│    dotMigrata Library        │
│    (This Package)               │
└─────────────────────────────────┘
```

### Integration Points

1. **Real-time Updates**: Use `ISimulationObserver` to stream events via SignalR/WebSocket
2. **State Management**: Use `XmlSnapshotSerializer` to save and restore simulation states
3. **Custom Stages**: Inject logging, metrics, or custom logic via `ISimulationStage`
4. **Serialization**: XML snapshots with namespace-based format for API integration

## Contributing

Contributions, bug reports, and feature requests are welcome. Please submit via GitHub issues or pull requests.

## License

Apache 2.0 - See LICENSE file for details.