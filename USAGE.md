# Usage Guide

This guide provides detailed examples and usage instructions for dotMigrata.

## Table of Contents

- [Installation](#installation)
- [Quick Start Example](#quick-start-example)
- [PersonCollection System](#personcollection-system)
- [Custom Person Generation](#custom-person-generation)
- [Creating Custom Person Types](#creating-custom-person-types)
- [Configuring Simulation Parameters](#configuring-simulation-parameters)
- [Working with Snapshots](#working-with-snapshots)

## Installation

### Adding to Your Project

Add the library to your .NET 8.0/9.0/10.0 project:

```bash
dotnet add package GeoMigrata.Framework
```

## Quick Start Example

Here's a step-by-step guide to get you started with dotMigrata:

### Step 1: Define Factors

Factors represent characteristics of cities (like income or pollution) that influence migration decisions. Define them
as `FactorDefinition` objects that will be referenced throughout your simulation.

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

// Define factor objects that will be referenced throughout the simulation
var incomeFactor = new FactorDefinition
{
    DisplayName = "Income",
    Type = FactorType.Positive,
    MinValue = 20000,
    MaxValue = 100000,
    Transform = null  // Linear normalization
};

var pollutionFactor = new FactorDefinition
{
    DisplayName = "Pollution",
    Type = FactorType.Negative,
    MinValue = 0,
    MaxValue = 100,
    Transform = null  // Linear normalization
};

var allFactors = new[] { incomeFactor, pollutionFactor };
```

### Step 2: Generate Population

Use `PersonCollection` to define how persons are generated. Notice how we reference the `FactorDefinition` objects
directly, not strings.

```csharp
var collection = new PersonCollection();
collection.Add(new GeneratorConfig
{
    Count = 100000,
    // Use FactorDefinition references (not strings) for type safety
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [incomeFactor] = ValueSpecification.InRange(3, 8),        // Sensitivity to income
        [pollutionFactor] = ValueSpecification.InRange(-7, -3)    // Negative sensitivity to pollution
    },
    // Direct ValueSpecification methods for person behavioral properties
    MovingWillingness = ValueSpecification.InRange(0.4, 0.7),
    RetentionRate = ValueSpecification.InRange(0.3, 0.6),
    Tags = ["urban_resident"]
});
```

### Step 3: Create Cities

Create cities with factor values and assign the generated population. Again, use `FactorDefinition` object references.

```csharp
var cityA = new City(
    factorValues: [
        new FactorValue { Definition = incomeFactor, Intensity = 50000 },    // Reference the object
        new FactorValue { Definition = pollutionFactor, Intensity = 30 }      // Not a string
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
    persons: [])  // Empty initially
{
    DisplayName = "City B",
    Location = new Coordinate { Latitude = 24.5, Longitude = 118.1 },
    Area = 80.0,
    Capacity = 800000
};
```

### Step 4: Create World

Combine cities and factor definitions into a world.

```csharp
var world = new World([cityA, cityB], allFactors)
{
    DisplayName = "Example World"
};
```

### Step 5: Configure Simulation Engine

Set up the simulation pipeline with calculators and observers.

```csharp
var attractionCalc = new StandardAttractionCalculator();
var migrationCalc = new StandardMigrationCalculator();

List<ISimulationStage> stages =
[
    new MigrationDecisionStage(migrationCalc, attractionCalc),
    new MigrationExecutionStage()
];

var engine = new SimulationEngine(stages, SimulationConfig.Default);
engine.AddObserver(new ConsoleObserver(colored: true));
```

### Step 6: Run Simulation

Execute the simulation and view results.

```csharp
var result = await engine.RunAsync(world);

Console.WriteLine($"Simulation completed after {result.CurrentTick} ticks");
Console.WriteLine($"Final population: {result.World.Population:N0} persons");
```

## Value Specifications

The framework provides several ways to specify values for person attributes:

### Fixed Values

```csharp
ValueSpecification.Fixed(0.75)  // All persons get exactly 0.75
```

### Range Values (Uniform Distribution)

```csharp
ValueSpecification.InRange(0.4, 0.8)  // Uniformly distributed between 0.4 and 0.8
```

### Approximate Values (Normal Distribution)

```csharp
// Values sampled from normal distribution with mean=0.6, stddev=0.15
ValueSpecification.Approximately(mean: 0.6, standardDeviation: 0.15)
```

### Random with Scale

```csharp
// Scale > 1.0 biases toward higher values, < 1.0 toward lower values
ValueSpecification.RandomWithScale(scale: 1.5)
```

## PersonCollection System

The **PersonCollection** system provides fine-grained control over population generation. You can add individual
persons,
duplicates, or use generators with specifications. **Important:** Always use `FactorDefinition` object references, not
strings.

```csharp
using dotMigrata.Core.Entities;
using dotMigrata.Core.Enums;
using dotMigrata.Core.Values;
using dotMigrata.Generator;

// First, define your factor objects
var incomeFactor = new FactorDefinition
{
    DisplayName = "Income",
    Type = FactorType.Positive,
    MinValue = 30000,
    MaxValue = 150000,
    Transform = null
};

var pollutionFactor = new FactorDefinition
{
    DisplayName = "Pollution",
    Type = FactorType.Negative,
    MinValue = 0,
    MaxValue = 100,
    Transform = null
};

var housingFactor = new FactorDefinition
{
    DisplayName = "Housing Cost",
    Type = FactorType.Negative,
    MinValue = 500,
    MaxValue = 3000,
    Transform = null
};

FactorDefinition[] allFactors = [incomeFactor, pollutionFactor, housingFactor];

// Create a PersonCollection with mixed specifications
var collection = new PersonCollection();

// 1. Add a specific individual with exact attributes
var wealthyPerson = new StandardPerson(new Dictionary<FactorDefinition, double>
{
    [incomeFactor] = 8.5,      // Use object reference, not string
    [pollutionFactor] = -6.0,
    [housingFactor] = -7.0
})
{
    MovingWillingness = NormalizedValue.FromRatio(0.85),
    RetentionRate = NormalizedValue.FromRatio(0.15),
    Tags = ["high_mobility", "wealthy"]
};
collection.Add(wealthyPerson);

// 2. Add 10,000 identical persons (duplicates)
var middleClassPerson = new StandardPerson(new Dictionary<FactorDefinition, double>
{
    [incomeFactor] = 5.0,
    [pollutionFactor] = -3.0,
    [housingFactor] = -4.0
})
{
    MovingWillingness = NormalizedValue.FromRatio(0.5),
    RetentionRate = NormalizedValue.FromRatio(0.5),
    Tags = ["middle_class"]
};
collection.Add(middleClassPerson, count: 10_000);

// 3. Generate 100,000 persons with varied attributes using a generator
collection.Add(new GeneratorConfig(seed: 42)
{
    Count = 100_000,
    // Use FactorDefinition references (not strings) for type safety
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [incomeFactor] = ValueSpecification.InRange(3, 15),  // Custom range for Income sensitivity
        [pollutionFactor] = ValueSpecification.Fixed(-5.0)      // Fixed value - all persons get -5.0
        // Note: housingFactor sensitivity will use default range with normal distribution
    },
    // Direct ValueSpecification methods for person behavioral properties
    MovingWillingness = ValueSpecification.InRange(0.6, 0.9),
    RetentionRate = ValueSpecification.InRange(0.3, 0.6),
    Tags = ["young_professional", "tech_worker"]
});

// Generate all persons and use in city
IEnumerable<PersonBase> persons = collection.GenerateAllPersons(allFactors);

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

- Mix individual persons, duplicates, and generators
- Support for tags to categorize and analyze populations
- Precise control with fixed values, custom ranges, or biased random
- Reproducible generation with seeds
- Efficient duplicate handling
- **Full-reference architecture:** Uses `FactorDefinition` object references for type safety

## Custom Person Generation

For more control over person attributes, you can configure individual generators with custom parameters:

```csharp
using dotMigrata.Core.Entities;
using dotMigrata.Core.Enums;
using dotMigrata.Core.Values;
using dotMigrata.Generator;

// Define factors first (full-reference architecture)
var incomeFactor = new FactorDefinition
{
    DisplayName = "Income",
    Type = FactorType.Positive,
    MinValue = 20000,
    MaxValue = 100000,
    Transform = null
};

FactorDefinition[] allFactors = [incomeFactor];

// Create PersonCollection with custom generator configuration
var collection = new PersonCollection();

// Configure generator with custom seed and sensitivity parameters
collection.Add(new GeneratorConfig(seed: 42)
{
    Count = 50000,
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [incomeFactor] = ValueSpecification.InRange(5, 9)  // Use FactorDefinition reference
    },
    // Direct ValueSpecification methods for person behavioral properties
    MovingWillingness = ValueSpecification.InRange(0.4, 0.7),
    RetentionRate = ValueSpecification.InRange(0.3, 0.6),
    // Advanced generator options
    DefaultSensitivityRange = new ValueRange(-10.0, 10.0),  // Default range for unspecified factors
    SensitivityStdDev = 3.0  // Standard deviation for normal distribution
});

IEnumerable<PersonBase> persons = collection.GenerateAllPersons(allFactors);

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

## Creating Custom Person Types

Version `0.5.0-beta` introduces an inheritance-based architecture that allows you to create custom person types by
inheriting from `PersonBase`. This enables domain-specific properties while maintaining compatibility with the
framework.

### When to Create Custom Person Types

Create a custom person type when you need:

- Domain-specific attributes (e.g., age, income, education level for demographic models)
- Custom behavioral properties beyond what `StandardPerson` provides
- Specialized migration logic in custom calculators

### Example: Creating a Demographic Person Type

```csharp
using dotMigrata.Core.Entities;
using dotMigrata.Core.Values;

/// <summary>
/// Custom person type with demographic attributes.
/// </summary>
public sealed class DemographicPerson : PersonBase
{
    public DemographicPerson(IDictionary<FactorDefinition, double> factorSensitivities)
        : base(factorSensitivities)
    {
    }

    /// <summary>
    /// Age of the person in years.
    /// </summary>
    public int Age { get; init; }

    /// <summary>
    /// Annual income in local currency.
    /// </summary>
    public double Income { get; init; }

    /// <summary>
    /// Education level (e.g., "HighSchool", "Bachelor", "Master", "PhD").
    /// </summary>
    public string EducationLevel { get; init; } = string.Empty;

    /// <summary>
    /// Employment status.
    /// </summary>
    public bool IsEmployed { get; init; }
}
```

### Using Custom Person Types with Custom Calculators

When using custom person types, you typically need custom calculators that can access the additional properties:

```csharp
using dotMigrata.Core.Entities;
using dotMigrata.Logic.Calculators;
using dotMigrata.Logic.Interfaces;
using dotMigrata.Logic.Models;

/// <summary>
/// Custom attraction calculator that considers demographic attributes.
/// </summary>
public class DemographicAttractionCalculator : IAttractionCalculator
{
    private readonly StandardAttractionCalculator _baseCalculator;

    public DemographicAttractionCalculator(StandardModelConfig? config = null)
    {
        _baseCalculator = new StandardAttractionCalculator(config);
    }

    public AttractionResult CalculateAttraction(City city, PersonBase person, City? originCity = null)
    {
        // Calculate base attraction using standard logic
        var result = _baseCalculator.CalculateAttraction(city, person, originCity);

        // Apply demographic-specific adjustments if it's a DemographicPerson
        if (person is DemographicPerson demoPerson)
        {
            var adjustment = 1.0;

            // Young people are more mobile (higher attraction)
            if (demoPerson.Age < 30)
                adjustment *= 1.2;

            // High-income individuals less sensitive to economic factors
            if (demoPerson.Income > 100000)
                adjustment *= 0.9;

            // Educated individuals prefer certain cities
            if (demoPerson.EducationLevel == "PhD" && city.DisplayName.Contains("University"))
                adjustment *= 1.3;

            result.AdjustedAttraction *= adjustment;
        }

        return result;
    }

    public IDictionary<City, AttractionResult> CalculateAttractionForAllCities(
        IEnumerable<City> cities, 
        PersonBase person, 
        City? originCity = null)
    {
        return cities.ToDictionary(
            city => city,
            city => CalculateAttraction(city, person, originCity));
    }
}
```

### Key Points for Custom Person Types

- **Inherit from `PersonBase`**: All custom person types must inherit from the abstract `PersonBase` class
- **Use pattern matching**: Access custom properties using `is` pattern matching in calculators
- **Thread safety**: Ensure custom properties are immutable (use `init` instead of `set`)
- **Tags support**: All person types inherit `Tags` property from `PersonBase` for consistent categorization
- **Factory pattern**: Consider creating factory methods for complex initialization

### Generating Custom Person Types with PersonFactory

Version `0.5.1-beta` adds support for generating custom person types using the `PersonFactory` property in
`GeneratorConfig`. This allows you to specify how custom properties should be generated for your custom person types.

```csharp
using dotMigrata.Core.Entities;
using dotMigrata.Core.Enums;
using dotMigrata.Core.Values;
using dotMigrata.Generator;

// Define your custom person type (as shown in previous example)
public sealed class DemographicPerson : PersonBase
{
    public DemographicPerson(IDictionary<FactorDefinition, double> factorSensitivities)
        : base(factorSensitivities)
    {
    }

    public int Age { get; init; }
    public double Income { get; init; }
    public string EducationLevel { get; init; } = string.Empty;
    public bool IsEmployed { get; init; }
}

// Define factors
var incomeFactor = new FactorDefinition
{
    DisplayName = "Income",
    Type = FactorType.Positive,
    MinValue = 20000,
    MaxValue = 100000
};

var educationFactor = new FactorDefinition
{
    DisplayName = "Education Quality",
    Type = FactorType.Positive,
    MinValue = 0,
    MaxValue = 100
};

FactorDefinition[] allFactors = [incomeFactor, educationFactor];

// Create a PersonCollection with custom person generator
var collection = new PersonCollection();

// Use PersonFactory to generate custom person types
var random = new Random(42);  // For additional custom property generation
collection.Add(new GeneratorConfig
{
    Count = 10000,
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [incomeFactor] = ValueSpecification.InRange(3, 8),
        [educationFactor] = ValueSpecification.InRange(2, 7)
    },
    MovingWillingness = ValueSpecification.InRange(0.4, 0.7),
    RetentionRate = ValueSpecification.InRange(0.3, 0.6),

    // PersonFactory receives: sensitivities, willingness, retention, scaling, threshold, minAttraction, tags
    // and returns a PersonBase-derived instance with custom properties set
    PersonFactory = (sensitivities, willingness, retention, scaling, threshold, minAttraction, tags) =>
    {
        // Generate custom properties
        var age = random.Next(18, 65);
        var income = random.Next(25000, 120000);
        var educationLevel = age < 25 ? "HighSchool" : 
                           age < 30 ? "Bachelor" :
                           age < 40 ? "Master" : "PhD";
        var isEmployed = random.NextDouble() > 0.1;  // 90% employment rate
        return new DemographicPerson(sensitivities)
        {
            MovingWillingness = willingness,
            RetentionRate = retention,
            Age = age,
            Income = income,
            EducationLevel = educationLevel,
            IsEmployed = isEmployed
        };
    }
});

// Generate all persons
IEnumerable<PersonBase> persons = collection.GenerateAllPersons(allFactors);

// You can now use these custom persons with your custom calculator
// Note: persons will be of type DemographicPerson, but returned as PersonBase
var demographicPersons = persons.Cast<DemographicPerson>();
Console.WriteLine($"Generated {demographicPersons.Count()} demographic persons");
Console.WriteLine($"Average age: {demographicPersons.Average(p => p.Age):F1}");
Console.WriteLine($"Average income: {demographicPersons.Average(p => p.Income):C0}");
```

**Important Notes for PersonFactory:**

- The `PersonFactory` function receives 7 parameters: factor sensitivities, moving willingness, retention rate,
  sensitivity scaling, attraction threshold, minimum acceptable attraction, and tags
- These are the standard properties that the generator creates based on your `ValueSpecification` settings
- Your factory function is responsible for creating the custom person instance and setting any additional custom
  properties
- You can use additional random number generators or any other logic within the factory to generate custom property
  values
- For StandardPerson (default), you can omit the `PersonFactory` and the framework will create StandardPerson instances
  automatically
- Template mode (`PersonCollection.Add(person, count)`) does not support custom person types - use `PersonFactory`
  instead

### Custom Person Types and Snapshots

Version `0.5.1-beta` makes the snapshot system compatible with custom person types by using `PersonBase` throughout.
However, note that:

- **XML snapshots store only StandardPerson properties** by default (willingness, retention, sensitivities, etc.)
- **Custom properties are not persisted** in the current snapshot format
- If you need to persist custom properties, you should:
  1. Store your world state using standard .NET serialization (JSON, Binary, etc.)
  2. Or extend the snapshot XML schema with custom elements for your person type
  3. Or use generators with PersonFactory to recreate custom persons from seed data

```csharp
using dotMigrata.Snapshot.Serialization;
using dotMigrata.Snapshot.Conversion;

// Converting a world with custom persons to snapshot
var world = new World(cities, allFactors)
{
    DisplayName = "World with Custom Persons"
};

// This will work, but custom properties won't be saved
var snapshot = SnapshotConverter.ToSnapshot(world);
XmlSnapshotSerializer.SerializeToFile(snapshot, "world-snapshot.xml");

// When loading back, you'll get base PersonBase instances
// Custom properties will be lost
var loadedSnapshot = XmlSnapshotSerializer.DeserializeFromFile("world-snapshot.xml");
var loadedWorld = SnapshotConverter.ToWorld(loadedSnapshot);

// For reproducible custom person generation, use generators with seeds
// Store the generator configuration instead of person instances
```

## Configuring Simulation Parameters

You can configure the simulation execution and model parameters using modern C# syntax:

```csharp
using dotMigrata.Logic.Calculators;
using dotMigrata.Logic.Models;
using dotMigrata.Simulation.Engine;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Models;
using dotMigrata.Simulation.Pipeline;

// Configure model parameters
StandardModelConfig modelConfig = new()
{
    CapacitySteepness = 5.0,
    DistanceDecayLambda = 0.001,
    MigrationProbabilitySteepness = 10.0,
    MigrationProbabilityThreshold = 0.0,
    FactorSmoothingAlpha = 0.2
};

// Configure simulation parameters
SimulationConfig simConfig = new()
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
List<ISimulationStage> stages =
[
    new MigrationDecisionStage(migrationCalc, attractionCalc),
    new MigrationExecutionStage()
];

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

Snapshots are typically created as XML files. Here's how to create a snapshot programmatically using modern C# syntax:

```csharp
using dotMigrata.Snapshot.Enums;
using dotMigrata.Snapshot.Models;
using dotMigrata.Snapshot.Serialization;

// Create snapshot structure
WorldSnapshotXml snapshot = new()
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
        FactorDefinitions =
        [
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
        ],
        
        // Define person collections
        PersonCollections =
        [
            new PersonCollectionXml
            {
                Id = "initial_population",
                Generators =
                [
                    new GeneratorXml
                    {
                        Count = 100000,
                        Seed = 42,
                        FactorSensitivities =
                        [
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
                        ],
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
                ]
            }
        ],
        
        // Define cities
        Cities =
        [
            new CityXml
            {
                Id = "city_a",
                DisplayName = "City A",
                Latitude = 26.0,
                Longitude = 119.3,
                Area = 100.0,
                Capacity = 1000000,
                FactorValues =
                [
                    new FactorValueXml { Id = "income", Value = 50000 },
                    new FactorValueXml { Id = "pollution", Value = 30 }
                ],
                PersonCollections =
                [
                    new CollectionRefXml { Id = "initial_population" }
                ]
            },
            new CityXml
            {
                Id = "city_b",
                DisplayName = "City B",
                Latitude = 24.5,
                Longitude = 118.1,
                Area = 80.0,
                Capacity = 800000,
                FactorValues =
                [
                    new FactorValueXml { Id = "income", Value = 40000 },
                    new FactorValueXml { Id = "pollution", Value = 20 }
                ]
            }
        ]
    }
};

// Save to file
XmlSnapshotSerializer.SerializeToFile(snapshot, "my-simulation-snapshot.xml");
Console.WriteLine("Snapshot saved to my-simulation-snapshot.xml");
```

### Converting Snapshots to World Objects

Use `SnapshotConverter` to convert snapshots to runnable World objects:

```csharp
using dotMigrata.Snapshot.Conversion;
using dotMigrata.Snapshot.Serialization;
using dotMigrata.Simulation.Builders;

// Load and convert snapshot
var snapshot = XmlSnapshotSerializer.DeserializeFromFile("examples/example-snapshot.xml");
var world = SnapshotConverter.ToWorld(snapshot!);

// Create simulation with fluent builder
var engine = SimulationBuilder.Create()
    .WithConsoleOutput(colored: true)
    .WithRandomSeed(42)
    .ConfigureSimulation(s => s.MaxTicks(100).StabilityThreshold(50))
    .ConfigureModel(m => m.CapacitySteepness(3.0))
    .Build();

// Run simulation
var result = await engine.RunAsync(world);
Console.WriteLine($"Final population: {result.World.Population:N0}");
```

### Exporting World to Snapshot

Convert simulation results back to snapshot format:

```csharp
using dotMigrata.Snapshot.Conversion;
using dotMigrata.Snapshot.Serialization;
using dotMigrata.Snapshot.Enums;

// After running simulation...
var outputSnapshot = SnapshotConverter.ToSnapshot(
    result.World,
    SnapshotStatus.Completed,
    currentStep: result.CurrentTick);

XmlSnapshotSerializer.SerializeToFile(outputSnapshot, "simulation-result.xml");
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

## Simulation Metrics

The framework provides comprehensive metrics collection for academic analysis:

```csharp
using dotMigrata.Simulation.Metrics;
using dotMigrata.Simulation.Builders;

// Create metrics observer
var metricsObserver = new MetricsObserver();

// Build simulation with metrics tracking
var engine = SimulationBuilder.Create()
    .WithConsoleOutput()
    .AddObserver(metricsObserver)
    .WithRandomSeed(42)
    .Build();

var result = await engine.RunAsync(world);

// Access comprehensive metrics
var collector = metricsObserver.Collector;
Console.WriteLine($"Total ticks: {collector.History.Count}");
Console.WriteLine($"Average migration rate: {collector.AverageMigrationRate:P2}");
Console.WriteLine($"Total migrations: {collector.TotalMigrations:N0}");

// Access per-tick statistics
var finalMetrics = collector.CurrentMetrics;
if (finalMetrics != null)
{
    Console.WriteLine($"Gini coefficient: {finalMetrics.PopulationGiniCoefficient:F4}");
    Console.WriteLine($"Population entropy: {finalMetrics.PopulationEntropy:F4}");
    Console.WriteLine($"Coefficient of variation: {finalMetrics.PopulationCoefficientOfVariation:F4}");
}

// Export to CSV for external analysis
File.WriteAllText("simulation_metrics.csv", collector.ExportToCsv());
```

### Available Metrics

**Per-tick metrics:**

- Migration count and rate
- Total population
- Gini coefficient (population inequality)
- Population entropy (distribution evenness)
- Coefficient of variation

**Per-city metrics:**

- Population and capacity utilization
- Incoming and outgoing migrations
- Net migration and population change

**Tag-based analysis:**

- Population counts by tag for demographic tracking