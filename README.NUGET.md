# dotMigrata

A .NET simulation framework for modeling individual-based population migration and city evolution in multi-city systems.

## Overview

dotMigrata simulates individual persons (10,000 to 1,000,000+) with unique characteristics, capturing how city factors
influence individual migration decisions and how migration reshapes city dynamics over time.

**Simulation Loop:**

```
City Factors → Individual Preferences → Migration Decisions → City Feedback → Iterative Evolution
```

## Key Features

### Individual-Based Simulation

- **10,000 to 1,000,000+ persons** with unique characteristics
- **Independent decision-making** based on personal preferences
- **Tag-based categorization** for statistical analysis
- **Reproducible simulations** using random seeds

### High Performance

- **PLINQ-based parallel processing** for multi-core optimization
- **Thread-safe operations** using concurrent collections
- **Scalable performance**: 10K-1M persons efficiently handled

### Flexible Architecture

- **Pipeline-based design** with custom simulation stages
- **Observer pattern** for real-time monitoring
- **Extensible calculators** for attraction and migration logic
- **Event system** for dynamic scenarios (policies, disasters, economic shifts)

### State Management

- **XML snapshot system** for deterministic reproducibility
- **Efficient serialization** supporting millions of persons
- **Step-based tracking** for simulation replay

## Installation

```bash
dotnet add package GeoMigrata.Framework
```

**Supported Frameworks:** .NET 8.0, .NET 9.0, .NET 10.0

## Quick Start

```csharp
using dotMigrata.Core.Entities;
using dotMigrata.Core.Enums;
using dotMigrata.Core.Values;
using dotMigrata.Generator;
using dotMigrata.Simulation.Builders;

// Define city factors (values are normalized 0-1)
var incomeFactor = new FactorDefinition
{
    DisplayName = "Income",
    Type = FactorType.Positive
};

var pollutionFactor = new FactorDefinition
{
    DisplayName = "Pollution",
    Type = FactorType.Negative
};

// Generate population
var collection = new PersonCollection();
collection.Add(new StandardPersonGenerator
{
    Count = 100000,
    FactorSensitivities = new Dictionary<FactorDefinition, UnitValuePromise>
    {
        [incomeFactor] = UnitValuePromise.InRange(0.3, 0.8),      // Sensitivity to income (0-1)
        [pollutionFactor] = UnitValuePromise.InRange(0.2, 0.6)    // Sensitivity to pollution (0-1)
    },
    MovingWillingness = UnitValuePromise.InRange(0.4, 0.7),
    RetentionRate = UnitValuePromise.InRange(0.3, 0.6)
});

// Create cities
var cityA = new City(
    factorIntensities: [
        new FactorIntensity { Definition = incomeFactor, Value = UnitValue.FromRatio(0.5) },
        new FactorIntensity { Definition = pollutionFactor, Value = UnitValue.FromRatio(0.3) }
    ],
    persons: collection.GenerateAllPersons([incomeFactor, pollutionFactor]))
{
    DisplayName = "City A",
    Location = new Coordinate { Latitude = 26.0, Longitude = 119.3 },
    Capacity = 1000000
};

// Create world and run simulation
var world = new World([cityA, cityB], [incomeFactor, pollutionFactor])
{
    DisplayName = "Migration Simulation"
};

var engine = SimulationBuilder.Create()
    .WithDisplay(DisplayPresets.Console)
    .ConfigureSimulation(s => s.MaxSteps(100))
    .Build();

var result = await engine.RunAsync(world);
await engine.DisposeAsync();
```

## Core Concepts

### World & Cities

- **World**: Top-level container for cities and factor definitions
- **City**: Contains factor values and individual persons
- **FactorDefinition**: Metadata for city characteristics (income, pollution, services)
- **FactorIntensity**: Actual values for each city factor

### Persons

- **PersonBase**: Abstract base class for all person types
- **StandardPerson**: Concrete implementation with migration-specific properties
- **PersonCollection**: Flexible population generation with specifications

### Simulation Engine

- **SimulationEngine**: Step-based orchestrator with pipeline architecture
- **ISimulationStage**: Extensible stage interface for custom logic
- **ISimulationObserver**: Observer pattern for real-time monitoring

### Events System

- **Event Triggers**: One-time, periodic, continuous, or conditional
- **Effect Types**: Absolute, delta, multiply, linear/logarithmic transitions
- **Use Cases**: Policy changes, disasters, economic shifts

## Architecture Layers

| Layer          | Description                                              |
|----------------|----------------------------------------------------------|
| **Core**       | Domain models: World, City, PersonBase, FactorDefinition |
| **Logic**      | Calculators: IAttractionCalculator, IMigrationCalculator |
| **Simulation** | Engine: SimulationEngine, pipeline stages, observers     |
| **Generator**  | Population: IPersonGenerator, StandardPersonGenerator    |
| **Snapshot**   | State management: XML serialization, reproducibility     |

## Performance Characteristics

| Scale      | Population | Memory    | Processing Time |
|------------|------------|-----------|-----------------|
| **Small**  | 10K - 50K  | 3-15 MB   | <1-3 sec/step   |
| **Medium** | 50K - 200K | 15-60 MB  | 3-10 sec/step   |
| **Large**  | 200K - 1M  | 60-300 MB | 10-90 sec/step  |

*Performance varies based on CPU cores, factor count, and city count*

## Extensibility

### Custom Person Types

```csharp
public class DemographicPerson : PersonBase
{
    public int Age { get; init; }
    public string EducationLevel { get; init; }
    // Add domain-specific properties
}
```

### Custom Calculators

```csharp
public class CustomAttractionCalculator : IAttractionCalculator
{
    public AttractionResult CalculateAttraction(City city, PersonBase person, City? originCity)
    {
        // Implement custom attraction logic
    }
}
```

### Custom Simulation Stages

```csharp
public class CustomStage : ISimulationStage
{
    public Task ExecuteAsync(SimulationContext context)
    {
        // Implement custom stage logic
    }
}
```

## Snapshot Management

Save and restore complete simulation states with deterministic reproducibility:

```csharp
using dotMigrata.Snapshot.Conversion;
using dotMigrata.Snapshot.Serialization;

// Export snapshot
var snapshot = SnapshotConverter.ToSnapshot(world, SnapshotStatus.Completed, currentStep: 100);
XmlSnapshotSerializer.SerializeToFile(snapshot, "simulation-state.xml");

// Load snapshot
var loadedSnapshot = XmlSnapshotSerializer.DeserializeFromFile("simulation-state.xml");
var world = SnapshotConverter.ToWorld(loadedSnapshot);
```

## Simulation Metrics

Comprehensive metrics for academic analysis:

```csharp
using dotMigrata.Simulation.Metrics;

var metricsObserver = new MetricsObserver();
var engine = SimulationBuilder.Create()
    .AddObserver(metricsObserver)
    .Build();

var result = await engine.RunAsync(world);
var metrics = metricsObserver.Collector;

Console.WriteLine($"Average migration rate: {metrics.AverageMigrationRate:P2}");
Console.WriteLine($"Gini coefficient: {metrics.CurrentMetrics?.PopulationGiniCoefficient:F4}");

File.WriteAllText("metrics.csv", metrics.ExportToCsv());
```

**Available Metrics:**

- Migration rates and counts per step
- Population distribution statistics (Gini, Entropy, Coefficient of Variation)
- Per-city metrics (incoming/outgoing migrations, capacity utilization)
- Tag-based population analysis

## Use Cases

- **Urban Planning**: Model population dynamics in response to infrastructure changes
- **Policy Analysis**: Simulate effects of economic policies on migration patterns
- **Research**: Study migration behavior in agent-based models
- **Education**: Teach complex systems and emergent behavior
- **Game Development**: Create realistic population dynamics for city-building games

## Documentation

- **[GitHub Repository](https://github.com/GeoMigrata/dotMigrata)** - Source code and examples
- **[dotMigrata - the framework from Project GeoMigrata](https://geomigrata.pages.dev/dotmigrata)** - Official website
- **[API Documentation](https://github.com/GeoMigrata/dotMigrata/blob/main/API.md)** - Complete API reference
- **[Usage Guide](https://github.com/GeoMigrata/dotMigrata/blob/main/USAGE.md)** - Detailed examples and tutorials

## Requirements

- **.NET 8.0, 9.0, or 10.0**
- **Dependencies**:
    - Microsoft.Extensions.DependencyInjection.Abstractions (10.0.1+)
    - Microsoft.Extensions.Logging.Abstractions (10.0.1+)

## License

Apache 2.0 - See [LICENSE](https://github.com/GeoMigrata/dotMigrata/blob/main/LICENSE) for details.

## Support

- **Issues**: [GitHub Issues](https://github.com/GeoMigrata/dotMigrata/issues)
- **Discussions**: [GitHub Discussions](https://github.com/GeoMigrata/dotMigrata/discussions)

---

**Project GeoMigrata** • Copyright © 2025 GeoMigrata