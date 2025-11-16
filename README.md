# dotMigrata

[![.NET8.0](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![.NET9.0](https://img.shields.io/badge/.NET-9.0-512BD4)](https://dotnet.microsoft.com/)
[![.NET10.0](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Apache2.0-blue.svg)](LICENSE)

dotMigrata, a product from Project GeoMigrata, is a C# .NET simulation framework designed to model individual-based population migration and city
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
dotnet add reference /path/to/dotMigrata.csproj
# Or, once published to NuGet:
# dotnet add package GeoMigrata.Framework
```

### Quick Start Example

Here's a simple example:

```csharp
using dotMigrata.Core.Entities;
using dotMigrata.Core.Enums;
using dotMigrata.Core.Values;
using dotMigrata.Generator;
using dotMigrata.Logic.Calculators;
using dotMigrata.Simulation.Engine;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;
using dotMigrata.Simulation.Pipeline;
using static dotMigrata.Generator.AttributeValueBuilder;

// Step 1: Define factors
var incomeFactor = new FactorDefinition
{
    DisplayName = "Income",
    Type = FactorType.Positive,
    MinValue = 20000,
    MaxValue = 100000
};

var pollutionFactor = new FactorDefinition
{
    DisplayName = "Pollution",
    Type = FactorType.Negative,
    MinValue = 0,
    MaxValue = 100
};

var allFactors = new[] { incomeFactor, pollutionFactor };

// Step 2: Generate persons using PersonCollection
var collection = new PersonCollection();
collection.Add(new GeneratorConfig
{
    Count = 100000,
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [incomeFactor] = Value().InRange(3, 8),
        [pollutionFactor] = Value().InRange(-7, -3)
    },
    MovingWillingness = Value().InRange(0.4, 0.7),
    RetentionRate = Value().InRange(0.3, 0.6),
    Tags = ["urban_resident"]
});

// Step 3: Create cities with factor values and persons
var cityA = new City(
    factorValues: [
        new FactorValue { Definition = incomeFactor, Intensity = 50000 },
        new FactorValue { Definition = pollutionFactor, Intensity = 30 }
    ],
    persons: collection.GenerateAllPersons(allFactors))
{
    DisplayName = "City A",
    Location = new Coordinate { Latitude = 26.0, Longitude = 119.3 },
    Area = 100.0,
    Capacity = 1000000
};

var cityB = new City(
    factorValues: [
        new FactorValue { Definition = incomeFactor, Intensity = 40000 },
        new FactorValue { Definition = pollutionFactor, Intensity = 20 }
    ],
    persons: []) // Empty initially
{
    DisplayName = "City B",
    Location = new Coordinate { Latitude = 24.5, Longitude = 118.1 },
    Area = 80.0,
    Capacity = 800000
};

// Step 4: Create world
var world = new World([cityA, cityB], allFactors)
{
    DisplayName = "Example World"
};

// Step 5: Create simulation engine
var attractionCalc = new StandardAttractionCalculator();
var migrationCalc = new StandardMigrationCalculator();

var stages = new List<ISimulationStage>
{
    new MigrationDecisionStage(migrationCalc, attractionCalc),
    new MigrationExecutionStage()
};

var engine = new SimulationEngine(stages, SimulationConfig.Default);
engine.AddObserver(new ConsoleObserver(colored: true));

// Step 6: Run simulation
var result = await engine.RunAsync(world);

Console.WriteLine($"Simulation completed after {result.CurrentTick} ticks");
Console.WriteLine($"Final population: {result.World.Population:N0} persons");
```

### Advanced Usage - PersonCollection

The **PersonCollection** system provides fine-grained control over population generation with support for Individual,
Individuals (duplicates), and Generator specifications:

```csharp
using dotMigrata.Generator;

// Create a PersonCollection with mixed specifications
var collection = new PersonCollection { IdPrefix = "CITY" };

// 1. Add specific individuals with exact attributes
collection.Add(new IndividualSpecification
{
    FactorSensitivities = new Dictionary<string, double>
    {
        ["Income"] = 8.5,
        ["Pollution"] = -6.0,
        ["Housing Cost"] = -7.0
    },
    MovingWillingness = 0.85,
    RetentionRate = 0.15,
    Tags = new[] { "high_mobility", "wealthy" }
});

// 2. Add 10,000 identical persons (duplicates)
collection.Add(new IndividualsSpecification
{
    Count = 10_000,
    FactorSensitivities = new Dictionary<string, double>
    {
        ["Income"] = 5.0,
        ["Pollution"] = -3.0
    },
    MovingWillingness = 0.5,
    RetentionRate = 0.5,
    Tags = new[] { "middle_class" }
});

// 3. Generate 100,000 persons with varied attributes
collection.Add(new GeneratorSpecification(seed: 42)
{
    Count = 100_000,
    FactorSensitivities = new Dictionary<string, ValueSpecification>
    {
        // Custom range for Income sensitivity
        ["Income"] = ValueSpecification.InRange(3, 15),
        // Fixed value - all persons get -5.0
        ["Pollution"] = ValueSpecification.Fixed(-5.0),
        // Random with bias (scale 1.2 = 20% higher on average)
        ["Housing Cost"] = ValueSpecification.Random().WithScale(1.2)
    },
    MovingWillingness = ValueSpecification.InRange(0.6, 0.9),
    Tags = new[] { "young_professional", "tech_worker" }
});

// Use PersonCollection in city
var persons = collection.GenerateAllPersons(allFactors);

var city = new City(
    factorValues: [
        new FactorValue { Definition = incomeFactor, Intensity = 80000 },
        new FactorValue { Definition = pollutionFactor, Intensity = 30 },
        new FactorValue { Definition = housingFactor, Intensity = 2500 }
    ],
    persons: persons)
{
    DisplayName = "City A",
    Location = new Coordinate { Latitude = 26.0, Longitude = 119.3 },
    Area = 100.0,
    Capacity = 500000
};

var world = new World([city], allFactors)
{
    DisplayName = "Multi-Cohort World"
};

// Analyze population by tags
var tagStats = world.AllPersons
    .SelectMany(p => p.Tags)
    .GroupBy(tag => tag)
    .Select(g => new { Tag = g.Key, Count = g.Count() });
```

**PersonCollection Benefits:**

- Mix Individual, Individuals, and Generator specifications
- Support for tags to categorize and analyze populations
- Precise control with fixed values, custom ranges, or biased random
- Reproducible generation with seeds
- Efficient duplicate handling

### Advanced Usage - Custom Person Generation

For more control over person attributes, you can configure the person generator:

```csharp
using dotMigrata.Generator;

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

// Create PersonCollection with custom configuration
var collection = new PersonCollection();
collection.Add(new GeneratorConfig
{
    Count = 50000,
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [incomeFactor] = Value().InRange(5, 9)
    },
    MovingWillingness = Value().InRange(0.4, 0.7),
    RetentionRate = Value().InRange(0.3, 0.6)
});

var persons = collection.GenerateAllPersons(allFactors, personConfig);

// Add persons to city
var city = new City(
    factorValues: [
        new FactorValue { Definition = incomeFactor, Intensity = 80000 }
    ],
    persons: persons)
{
    DisplayName = "City A",
    Location = new Coordinate { Latitude = 26.0, Longitude = 119.3 },
    Area = 100.0,
    Capacity = 500000
};
```

### Configuring Simulation Parameters

You can also configure the simulation execution and model parameters:

```csharp
using dotMigrata.Logic.Models;
using dotMigrata.Simulation.Models;

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

// Create calculators with custom configuration
var attractionCalc = new StandardAttractionCalculator(modelConfig);
var migrationCalc = new StandardMigrationCalculator(modelConfig);

// Create simulation engine with custom configuration
var stages = new List<ISimulationStage>
{
    new MigrationDecisionStage(migrationCalc, attractionCalc),
    new MigrationExecutionStage()
};

var engine = new SimulationEngine(stages, simConfig);
engine.AddObserver(new ConsoleObserver(colored: true));

// Run simulation
var result = await engine.RunAsync(world);
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

Complete snapshot system for saving and restoring simulation states with PersonCollection-based architecture:

- **XML Serialization** - Attribute-based XML format using `System.Xml.Serialization` with namespace support
- **PersonCollection Storage** - Stores collection specifications (templates + generators) instead of individual persons
- **Deterministic Reproducibility** - Uses random seeds to regenerate exact simulation states
- **Namespace Design** - Distinguishes code concepts (`c:Person`, `c:City`) from snapshot containers
- **Efficient Format** - Compact XML with attributes for simple values, elements for complex structures

**Key Features:**

- PersonCollections are permanent snapshot data (like FactorDefinitions)
- Persons regenerated from specifications on load (immutable properties)
- Step-based state tracking for simulation reproducibility
- Collections "expanded" at simulation start (generators produce persons)
- No individual person serialization (enables millions of persons)

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

- **`World`**, **`City`**, **`Person`** - Core simulation entities
- **`FactorDefinition`**, **`FactorValue`** - City characteristic system
- **`PersonGenerator`**, **`PersonGeneratorConfig`** - Population generation
- **`SimulationContext`** - Runtime simulation state
- **`AttractionResult`**, **`MigrationFlow`** - Calculation results

## Examples

See the `/examples` directory for complete working examples:

- **`PersonBasedSimulationExample.cs`** - Complete person-based simulation with 230,000 persons across 3 cities
- **`example-snapshot.xml`** - Example XML snapshot with PersonCollection architecture and namespace design
- **`README.md`** - Detailed explanation of features and PersonCollection usage

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
2. **State Management**: Use `XmlSnapshotSerializer` to save and restore simulation states with PersonCollection
   specifications
3. **Custom Stages**: Inject logging, metrics, or custom logic via `ISimulationStage`
4. **Serialization**: XML snapshots with namespace-based format for API integration and deterministic reproducibility

## Contributing

Contributions, bug reports, and feature requests are welcome. Please submit via GitHub issues or pull requests.

## License

Apache 2.0 - See LICENSE file for details.