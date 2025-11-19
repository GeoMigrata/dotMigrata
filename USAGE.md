# Usage Guide

This guide provides detailed examples and usage instructions for dotMigrata.

## Table of Contents

- [Installation](#installation)
- [Quick Start Example](#quick-start-example)
- [PersonCollection System](#personcollection-system)
- [Custom Person Generation](#custom-person-generation)
- [Configuring Simulation Parameters](#configuring-simulation-parameters)
- [Working with Snapshots](#working-with-snapshots)
- [Examples](#examples)

## Installation

### Adding to Your Project

Add the library to your .NET 8.0/9.0/10.0 project:

```bash
dotnet add reference /path/to/dotMigrata.csproj
# Or, once published to NuGet:
# dotnet add package GeoMigrata.Framework
```

## Quick Start Example

Here's a simple example to get you started:

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

## PersonCollection System

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

## Custom Person Generation

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

## Configuring Simulation Parameters

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

## Working with Snapshots

The snapshot system provides XML-based serialization for saving and loading simulation configurations. Snapshots use
PersonCollection specifications for efficient storage and deterministic reproducibility.

### Loading a Snapshot from File

```csharp
using dotMigrata.Snapshot.Serialization;

// Deserialize snapshot from XML file
var snapshot = XmlSnapshotSerializer.DeserializeFromFile("examples/example-snapshot.xml");

if (snapshot?.World != null)
{
    Console.WriteLine($"Loaded snapshot: {snapshot.World.DisplayName}");
    Console.WriteLine($"Status: {snapshot.Status}");
    Console.WriteLine($"Current Step: {snapshot.CurrentStep}");
    Console.WriteLine($"Cities: {snapshot.World.Cities?.Count ?? 0}");
    Console.WriteLine($"Factor Definitions: {snapshot.World.FactorDefinitions?.Count ?? 0}");
    Console.WriteLine($"Person Collections: {snapshot.World.PersonCollections?.Count ?? 0}");
}
```

### Creating and Saving a Snapshot

Snapshots are typically created as XML files. Here's how to create a snapshot programmatically:

```csharp
using dotMigrata.Snapshot.Models;
using dotMigrata.Snapshot.Serialization;
using dotMigrata.Snapshot.Enums;

// Create snapshot structure
var snapshot = new WorldSnapshotXml
{
    Version = "1.0",
    Status = SnapshotStatus.Seed,
    CreatedAt = DateTime.UtcNow,
    LastModifiedAt = DateTime.UtcNow,
    CurrentStep = 0,
    World = new WorldStateXml
    {
        DisplayName = "My Simulation World",
        
        // Define factors
        FactorDefinitions = new List<FactorDefXml>
        {
            new FactorDefXml
            {
                Id = "income",
                DisplayName = "Income",
                Type = "Positive",
                Min = 20000,
                Max = 100000,
                Transform = "Linear"
            },
            new FactorDefXml
            {
                Id = "pollution",
                DisplayName = "Pollution",
                Type = "Negative",
                Min = 0,
                Max = 100,
                Transform = "Linear"
            }
        },
        
        // Define person collections
        PersonCollections = new List<PersonCollectionXml>
        {
            new PersonCollectionXml
            {
                Id = "initial_population",
                Generators = new List<GeneratorXml>
                {
                    new GeneratorXml
                    {
                        Count = 100000,
                        Seed = 42,
                        FactorSensitivities = new List<SensitivitySpecXml>
                        {
                            new SensitivitySpecXml
                            {
                                Id = "income",
                                InRange = new RangeValueXml { Min = 3, Max = 8 }
                            },
                            new SensitivitySpecXml
                            {
                                Id = "pollution",
                                InRange = new RangeValueXml { Min = -7, Max = -3 }
                            }
                        },
                        MovingWillingness = new ValueSpecXml
                        {
                            InRange = new RangeValueXml { Min = 0.4, Max = 0.7 }
                        },
                        RetentionRate = new ValueSpecXml
                        {
                            InRange = new RangeValueXml { Min = 0.3, Max = 0.6 }
                        },
                        Tags = "urban_resident"
                    }
                }
            }
        },
        
        // Define cities
        Cities = new List<CityXml>
        {
            new CityXml
            {
                Id = "city_a",
                DisplayName = "City A",
                Latitude = 26.0,
                Longitude = 119.3,
                Area = 100.0,
                Capacity = 1000000,
                FactorValues = new List<FactorValueXml>
                {
                    new FactorValueXml { Id = "income", Value = 50000 },
                    new FactorValueXml { Id = "pollution", Value = 30 }
                },
                PersonCollections = new List<CollectionRefXml>
                {
                    new CollectionRefXml { Id = "initial_population" }
                }
            },
            new CityXml
            {
                Id = "city_b",
                DisplayName = "City B",
                Latitude = 24.5,
                Longitude = 118.1,
                Area = 80.0,
                Capacity = 800000,
                FactorValues = new List<FactorValueXml>
                {
                    new FactorValueXml { Id = "income", Value = 40000 },
                    new FactorValueXml { Id = "pollution", Value = 20 }
                }
            }
        }
    }
};

// Save to file
XmlSnapshotSerializer.SerializeToFile(snapshot, "my-simulation-snapshot.xml");
Console.WriteLine("Snapshot saved to my-simulation-snapshot.xml");
```

### Snapshot Benefits

- **Deterministic Reproducibility**: Same seed + step count = exact same state
- **Efficient Storage**: Stores PersonCollection specifications instead of individual persons
- **Human Readable**: XML format is easy to inspect and modify
- **Version Control Friendly**: Text-based format works well with Git
- **Scalable**: Supports millions of persons without storing individual instances

### Snapshot Structure

A snapshot contains:

- **FactorDefinitions**: Global factor definitions used across all cities
- **PersonCollections**: Reusable population specifications (templates and generators)
- **Cities**: City definitions with factor values and person collection references
- **Steps**: Optional simulation steps for reproducibility

See [examples/example-snapshot.xml](../examples/example-snapshot.xml) for a complete working example.

## Examples

See the `/examples` directory for complete working examples:

- **`PersonBasedSimulationExample.cs`** - Complete person-based simulation with 230,000 persons across 3 cities
- **`example-snapshot.xml`** - Example XML snapshot with PersonCollection architecture and namespace design
- **`README.md`** - Detailed explanation of features and PersonCollection usage

For detailed API documentation, see [API.md](API.md).