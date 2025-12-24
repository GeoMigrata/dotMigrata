# Usage Guide

This guide provides detailed examples and usage instructions for dotMigrata.

## Table of Contents

- [Installation](#installation)
- [Quick Start Example](#quick-start-example)
- [Value Specifications](#value-specifications)
- [PersonCollection System](#personcollection-system)
- [Creating Custom Person Types](#creating-custom-person-types)
- [Working with Snapshots](#working-with-snapshots)
- [Event System](#event-system)
- [Configuring Simulation Parameters](#configuring-simulation-parameters)
- [Simulation Metrics](#simulation-metrics)
- [Performance Optimization](#performance-optimization)

## Installation

### Adding to Your Project

Add the library to your .NET 8.0/9.0/10.0 project:

```bash
dotnet add package GeoMigrata.Framework
```

## Quick Start Example

Here's a step-by-step guide to get you started with dotMigrata:

**Required Namespaces:**

```csharp
using dotMigrata.Core.Entities;
using dotMigrata.Core.Enums;
using dotMigrata.Core.Values;
using dotMigrata.Generator;
using dotMigrata.Simulation.Builders;
```

### Step 1: Define Factors

Factors represent characteristics of cities (like income or pollution) that influence migration decisions. Define them
as `FactorDefinition` objects that will be referenced throughout your simulation.

**Note:** Factor intensities are stored as normalized `UnitValue` (0-1 range).

```csharp
var incomeFactor = new FactorDefinition
{
    DisplayName = "Income",
    Type = FactorType.Positive,
    TransformFunction = null  // Optional transformation
};

var pollutionFactor = new FactorDefinition
{
    DisplayName = "Pollution",
    Type = FactorType.Negative,
    TransformFunction = null
};

var allFactors = new[] { incomeFactor, pollutionFactor };
```

### Step 2: Generate Population

Use `PersonCollection` to define how persons are generated.

```csharp
var collection = new PersonCollection();
collection.Add(new StandardPersonGenerator
{
    Count = 10_0000,
    // Use FactorDefinition references for type safety
    FactorSensitivities = new Dictionary<FactorDefinition, UnitValuePromise>
    {
        [incomeFactor] = UnitValuePromise.InRange(0.3, 0.8),        // Sensitivity to income (0-1)
        [pollutionFactor] = UnitValuePromise.InRange(0.2, 0.6)      // Sensitivity to pollution (0-1)
    },
    // Person behavioral properties
    MovingWillingness = UnitValuePromise.InRange(0.4, 0.7),
    RetentionRate = UnitValuePromise.InRange(0.3, 0.6),
    Tags = ["urban_resident"]
});
```

### Step 3: Create Cities

Create cities with factor values and assign the generated population.

```csharp
var cityA = new City(
    factorIntensities: [
        new FactorIntensity { Definition = incomeFactor, Value = UnitValue.FromRatio(0.5) },
        new FactorIntensity { Definition = pollutionFactor, Value = UnitValue.FromRatio(0.3) }
    ],
    persons: collection.GenerateAllPersons(allFactors))
{
    DisplayName = "City A",
    Location = new Coordinate { Latitude = 26.0, Longitude = 119.3 },
    Area = 100.0,
    Capacity = 1_000_000
};

var cityB = new City(
    factorIntensities: [
        new FactorIntensity { Definition = incomeFactor, Value = UnitValue.FromRatio(0.4) },
        new FactorIntensity { Definition = pollutionFactor, Value = UnitValue.FromRatio(0.2) }
    ],
    persons: [])  // Empty initially
{
    DisplayName = "City B",
    Location = new Coordinate { Latitude = 24.5, Longitude = 118.1 },
    Area = 80.0,
    Capacity = 800_000
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

Set up the simulation using the fluent builder API (recommended approach).

```csharp
var engine = SimulationBuilder.Create()
    .WithConsoleOutput()
    .ConfigureSimulation(s => s.MaxSteps(100))
    .Build();
```

Alternatively, for advanced scenarios with custom calculators:

```csharp
var attractionCalc = new StandardAttractionCalculator();
using var migrationCalc = new StandardMigrationCalculator();

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

Console.WriteLine($"Simulation completed after {result.CurrentStep} steps");
Console.WriteLine($"Final population: {result.World.Population:N0} persons");

// Dispose the engine to release resources
await engine.DisposeAsync();
```

## Value Specifications

The framework provides `UnitValuePromise` for specifying values in generators with lazy evaluation:

### Fixed Values

```csharp
UnitValuePromise.Fixed(0.75)  // All persons get exactly 0.75
```

### Range Values (Uniform Distribution)

```csharp
UnitValuePromise.InRange(0.4, 0.8)  // Uniformly distributed between 0.4 and 0.8
```

### Approximate Values (Normal Distribution)

```csharp
// Values sampled from normal distribution with mean=0.6, stddev=0.15
UnitValuePromise.Approximately(mean: 0.6, standardDeviation: 0.15)
```

### Random with Scale

```csharp
// Scale > 1.0 biases toward higher values, < 1.0 toward lower values
UnitValuePromise.RandomWithScale(scale: 1.5)
```

For direct value assignment (not in generators), use `UnitValue`:

```csharp
var cityFactor = new FactorIntensity 
{ 
    Definition = incomeFactor, 
    Value = UnitValue.FromRatio(0.5) 
};
```

## PersonCollection System

The **PersonCollection** system gives you fine-grained control over population generation. Add individual persons,
duplicates, or use generators with specifications.

```csharp
// First, define your factor objects
var incomeFactor = new FactorDefinition
{
    DisplayName = "Income",
    Type = FactorType.Positive,
    TransformFunction = null
};

var pollutionFactor = new FactorDefinition
{
    DisplayName = "Pollution",
    Type = FactorType.Negative,
    TransformFunction = null
};

var housingFactor = new FactorDefinition
{
    DisplayName = "Housing Cost",
    Type = FactorType.Negative,
    TransformFunction = null
};

FactorDefinition[] allFactors = [incomeFactor, pollutionFactor, housingFactor];

// Create a PersonCollection with mixed specifications
var collection = new PersonCollection();

// 1. Add a specific individual with exact attributes
var wealthyPerson = new StandardPerson(new Dictionary<FactorDefinition, UnitValue>
{
    [incomeFactor] = UnitValue.FromRatio(0.85),
    [pollutionFactor] = UnitValue.FromRatio(0.2),
    [housingFactor] = UnitValue.FromRatio(0.3)
})
{
    MovingWillingness = UnitValue.FromRatio(0.85),
    RetentionRate = UnitValue.FromRatio(0.15),
    Tags = ["high_mobility", "wealthy"]
};
collection.Add(wealthyPerson);

// 2. Add 10,000 identical persons (duplicates)
var middleClassPerson = new StandardPerson(new Dictionary<FactorDefinition, UnitValue>
{
    [incomeFactor] = UnitValue.FromRatio(0.5),
    [pollutionFactor] = UnitValue.FromRatio(0.4),
    [housingFactor] = UnitValue.FromRatio(0.4)
})
{
    MovingWillingness = UnitValue.FromRatio(0.5),
    RetentionRate = UnitValue.FromRatio(0.5),
    Tags = ["middle_class"]
};
collection.Add(middleClassPerson, count: 10_000);

// 3. Generate 100,000 persons with varied attributes using a generator
collection.Add(new StandardPersonGenerator(seed: 42)
{
    Count = 100_000,
    FactorSensitivities = new Dictionary<FactorDefinition, UnitValuePromise>
    {
        [incomeFactor] = UnitValuePromise.InRange(0.3, 0.9),
        [pollutionFactor] = UnitValuePromise.Fixed(0.5)
    },
    MovingWillingness = UnitValuePromise.InRange(0.6, 0.9),
    RetentionRate = UnitValuePromise.InRange(0.3, 0.6),
    Tags = ["young_professional", "tech_worker"]
});

IEnumerable<PersonBase> persons = collection.GenerateAllPersons(allFactors);

var city = new City(
    factorIntensities: [
        new FactorIntensity { Definition = incomeFactor, Value = UnitValue.FromRatio(0.8) },
        new FactorIntensity { Definition = pollutionFactor, Value = UnitValue.FromRatio(0.3) },
        new FactorIntensity { Definition = housingFactor, Value = UnitValue.FromRatio(0.25) }
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
- Type-safe FactorDefinition references

## Creating Custom Person Types

The framework introduces an inheritance-based architecture for creating custom person types by
inheriting from `PersonBase`. This lets you add domain-specific properties while maintaining framework compatibility.

### When to Create Custom Person Types

Create a custom person type when you need:

- Domain-specific attributes (e.g., age, income, education level for demographic models)
- Custom behavioral properties beyond what `StandardPerson` provides
- Specialized migration logic in custom calculators

### Example: Creating a Demographic Person Type

```csharp
/// <summary>
/// Custom person type with demographic attributes.
/// </summary>
public sealed class DemographicPerson : PersonBase
{
    public DemographicPerson(IDictionary<FactorDefinition, UnitValue> factorSensitivities)
        : base(factorSensitivities)
    {
    }

    /// <summary>Age of the person in years.</summary>
    public int Age { get; init; }

    /// <summary>Annual income in local currency.</summary>
    public double Income { get; init; }

    /// <summary>Education level (e.g., "HighSchool", "Bachelor", "Master", "PhD").</summary>
    public string EducationLevel { get; init; } = string.Empty;

    /// <summary>Employment status.</summary>
    public bool IsEmployed { get; init; }
}
```

### Using Custom Person Types with Custom Calculators

When using custom person types, you'll typically need custom calculators to access the additional properties:

```csharp
/// <summary>
/// Custom attraction calculator that considers demographic attributes.
/// </summary>
/// <remarks>
/// Note: With World's person type validation, all persons will be DemographicPerson
/// when using this calculator, so pattern matching is safe.
/// </remarks>
public class DemographicAttractionCalculator : IAttractionCalculator
{
    private readonly StandardAttractionCalculator _baseCalculator;

    public DemographicAttractionCalculator(StandardModelConfig? config = null) =>
        _baseCalculator = new StandardAttractionCalculator(config);

    public AttractionResult CalculateAttraction(City city, PersonBase person, City? originCity = null)
    {
        // Calculate base attraction using standard logic
        var result = _baseCalculator.CalculateAttraction(city, person, originCity);

        // Since World validates all persons are the same type, we can safely cast
        var demoPerson = (DemographicPerson)person;
        
        var adjustment = 1.0;

        // Young people are more mobile (higher attraction)
        if (demoPerson.Age < 30)
            adjustment *= 1.2;

        // High-income individuals less sensitive to economic factors
        if (demoPerson.Income > 10_0000)
            adjustment *= 0.9;

        // Educated individuals prefer certain cities
        if (demoPerson.EducationLevel == "PhD" && city.DisplayName.Contains("University"))
            adjustment *= 1.3;

        result.AdjustedAttraction *= adjustment;
        return result;
    }

    public IDictionary<City, AttractionResult> CalculateAttractionForAllCities(
        IEnumerable<City> cities, 
        PersonBase person, 
        City? originCity = null) => 
            cities.ToDictionary(
            city => city,
            city => CalculateAttraction(city, person, originCity));
}
```

### Key Points for Custom Person Types

- **Inherit from `PersonBase`**: All custom person types must inherit from the abstract `PersonBase` class
- **Implement `IPersonGenerator<TPerson>`**: Create a custom generator for your person type
- **Single type per world**: The framework enforces that all persons in a simulation are the same type
- **Thread safety**: Ensure custom properties are immutable (use `init` instead of `set`)
- **Tags support**: All person types inherit `Tags` property from `PersonBase` for consistent categorization
- **Factory pattern**: Consider creating factory methods for complex initialization

### Generating Custom Person Types

The framework provides a type-safe way to generate custom person types by implementing the
`IPersonGenerator<TPerson>` interface.

```csharp
// Define your custom person type
public sealed class DemographicPerson : PersonBase
{
    public DemographicPerson(IDictionary<FactorDefinition, UnitValue> factorSensitivities)
        : base(factorSensitivities)
    {
    }

    public int Age { get; init; }
    public double Income { get; init; }
    public string EducationLevel { get; init; } = string.Empty;
    public bool IsEmployed { get; init; }
}

// Implement a custom generator for your person type
public sealed class DemographicPersonGenerator : IPersonGenerator<DemographicPerson>
{
    private readonly Random _random;

    public DemographicPersonGenerator(int seed = 0) =>
        _random = seed == 0 ? new Random() : new Random(seed);

    public required int Count { get; init; }
    public required UnitValuePromise MovingWillingness { get; init; }
    public required UnitValuePromise RetentionRate { get; init; }
    public required UnitValuePromise Age { get; init; }
    public required UnitValuePromise Income { get; init; }
    public Dictionary<FactorDefinition, UnitValuePromise> FactorSensitivities { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];

    public IEnumerable<DemographicPerson> Generate(IEnumerable<FactorDefinition> factorDefinitions)
    {
        var factors = factorDefinitions.ToList();
        for (var i = 0; i < Count; i++)
        {
            // Generate base properties
            var sensitivities = new Dictionary<FactorDefinition, UnitValue>();
            foreach (var factor in factors)
            {
                if (FactorSensitivities.TryGetValue(factor, out var spec))
                    sensitivities[factor] = spec.Generate(_random);
                else
                    sensitivities[factor] = UnitValue.FromRatio(_random.NextDouble());
            }

            var age = (int)Age.Generate(_random).Value;
            var income = Income.Generate(_random).Value;
            var educationLevel = age switch
            {
                < 25 => "HighSchool",
                < 30 => "Bachelor",
                < 40 => "Master",
                _ => "PhD"
            };

            yield return new DemographicPerson(sensitivities)
            {
                MovingWillingness = MovingWillingness.Generate(_random),
                RetentionRate = RetentionRate.Generate(_random),
                Age = age,
                Income = income,
                EducationLevel = educationLevel,
                IsEmployed = _random.NextDouble() > 0.1,
                Tags = Tags.ToList()
            };
        }
    }
}

// Usage
var incomeFactor = new FactorDefinition
{
    DisplayName = "Income",
    Type = FactorType.Positive,
    TransformFunction = null
};

FactorDefinition[] allFactors = [incomeFactor];

var collection = new PersonCollection();
collection.Add(new DemographicPersonGenerator(seed: 42)
{
    Count = 10_000,
    FactorSensitivities = new Dictionary<FactorDefinition, UnitValuePromise>
    {
        [incomeFactor] = UnitValuePromise.InRange(0.3, 0.8)
    },
    MovingWillingness = UnitValuePromise.InRange(0.4, 0.7),
    RetentionRate = UnitValuePromise.InRange(0.3, 0.6),
    Age = UnitValuePromise.InRange(18, 65),
    Income = UnitValuePromise.InRange(25000, 120000),
    Tags = ["demographic_study"]
});

IEnumerable<PersonBase> persons = collection.GenerateAllPersons(allFactors);
var demographicPersons = persons.Cast<DemographicPerson>();
Console.WriteLine($"Generated {demographicPersons.Count()} demographic persons");
Console.WriteLine($"Average age: {demographicPersons.Average(p => p.Age):F1}");
```

**Benefits of Custom Generators:**

- **Type-safe**: Define exactly the properties your person type needs
- **Flexible**: Full control over generation logic
- **Clean API**: No unwieldy multi-parameter functions
- **Testable**: Easy to unit test generator logic
- **Discoverable**: Clear interface makes implementation obvious

### Custom Person Types and Snapshots

The snapshot system supports custom person types through a converter-based approach.

#### Default Behavior

Snapshots default to `StandardPerson`:

```csharp
// Converting a world to snapshot
var snapshot = SnapshotConverter.ToSnapshot(world);
XmlSnapshotSerializer.SerializeToFile(snapshot, "world-snapshot.xml");

// Loading back - creates StandardPerson instances by default
var loadedSnapshot = XmlSnapshotSerializer.DeserializeFromFile("world-snapshot.xml");
var loadedWorld = SnapshotConverter.ToWorld(loadedSnapshot);
```

#### Custom Person Types

To support custom person types in snapshots:

1. Define your custom person model class (implement `IPersonModel`)
2. Implement `IPersonModelConverter<TModel, TPerson, TGenerator>` for type-safe conversion
3. Pass the converter to snapshot operations

```csharp
// 1. Define your person model
[XmlRoot("Person")]
public class DemographicPersonModel : IPersonModel
{
    [XmlAttribute("Count")] public int Count { get; set; } = 1;
    [XmlAttribute("Seed")] public int Seed { get; set; }
    [XmlIgnore] public bool SeedSpecified { get; set; }
    [XmlAttribute("Type")] public string Type { get; set; } = "DemographicPerson";
    [XmlElement("Tags")] public string? Tags { get; set; }
    [XmlIgnore] public bool IsGenerator => SeedSpecified;
    
    // Custom properties
    [XmlElement("Age")] public int Age { get; set; }
    [XmlElement("Income")] public double Income { get; set; }
}

// 2. Implement the converter
public class DemographicPersonConverter 
    : IPersonModelConverter<DemographicPersonModel, DemographicPerson, DemographicPersonGenerator>
{
    public DemographicPerson CreatePerson(
        DemographicPersonModel model,
        Dictionary<string, FactorDefinition> factorLookup,
        List<FactorDefinition> allFactors)
    {
        var sensitivities = ConvertSensitivities(model.Sensitivities, factorLookup);
        var tags = ParseTags(model.Tags);
        
        return new DemographicPerson(sensitivities)
        {
            MovingWillingness = ConvertToUnitValue(model.Willingness, 0.5),
            RetentionRate = ConvertToUnitValue(model.Retention, 0.3),
            Age = model.Age,
            Income = model.Income,
            Tags = tags
        };
    }
    
    public DemographicPersonGenerator CreateGenerator(
        DemographicPersonModel model,
        Dictionary<string, FactorDefinition> factorLookup,
        List<FactorDefinition> allFactors)
    {
        var factorSpecs = ConvertToUnitValuePromise(model.Sensitivities, factorLookup);
        var seed = model.SeedSpecified ? model.Seed : Random.Shared.Next();
        
        return new DemographicPersonGenerator(seed)
        {
            Count = model.Count,
            FactorSensitivities = factorSpecs,
            MovingWillingness = ConvertToPromise(model.Willingness, 0.5),
            RetentionRate = ConvertToPromise(model.Retention, 0.3)
        };
    }
    
    public DemographicPersonModel ToModel(DemographicPerson person) { /* ... */ }
    public DemographicPersonModel ToModel(DemographicPersonGenerator generator) { /* ... */ }
}

// 3. Use the converter with snapshot operations
var converter = new DemographicPersonConverter();
var snapshot = SnapshotConverter.ToSnapshot(world, converter);
var loadedWorld = SnapshotConverter.ToWorld(snapshot, converter);
```

## Working with Snapshots

The snapshot system provides XML-based serialization for saving and loading simulation configurations. Snapshots store
PersonCollection specifications for efficient storage and deterministic reproducibility.

### Loading a Snapshot from File

```csharp
// Deserialize snapshot from XML file
var snapshot = XmlSnapshotSerializer.DeserializeFromFile("examples/example-snapshot-v4.xml");

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

Snapshots are typically created as XML files. Here's how to create one programmatically using modern C# syntax:

```csharp
// Create snapshot structure
WorldSnapshotXml snapshot = new()
{
    Version = "beta",
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
                Max = 10_0000,
                CustomTransformName = "Linear"
            },
            new FactorDefXml
            {
                Id = "pollution",
                DisplayName = "Pollution",
                Type = "Negative",
                Min = 0,
                Max = 100,
                CustomTransformName = "Linear"
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
                        Count = 10_0000,
                        Seed = 42,
                        FactorSensitivities =
                        [
                            new SensitivitySpecXml
                            {
                                Id = "income",
                                InRange = new RangeValueXml { Min = 0.3, Max = 0.8 }
                            },
                            new SensitivitySpecXml
                            {
                                Id = "pollution",
                                InRange = new RangeValueXml { Min = 0.2, Max = 0.6 }
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
                Capacity = 10_00000,
                FactorValues =
                [
                    new FactorIntensityXml { Id = "income", Value = 50000 },
                    new FactorIntensityXml { Id = "pollution", Value = 30 }
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
                    new FactorIntensityXml { Id = "income", Value = 40000 },
                    new FactorIntensityXml { Id = "pollution", Value = 20 }
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
// Load and convert snapshot
var snapshot = XmlSnapshotSerializer.DeserializeFromFile("examples/example-snapshot-v4.xml");
var world = SnapshotConverter.ToWorld(snapshot!);

// Create simulation with fluent builder
var engine = SimulationBuilder.Create()
    .WithConsoleOutput(colored: true)
    .WithRandomSeed(42)
    .ConfigureSimulation(s => s.MaxSteps(100).StabilityThreshold(50))
    .ConfigureModel(m => m.CapacitySteepness(3.0))
    .Build();

// Run simulation
var result = await engine.RunAsync(world);
```

### Exporting World to Snapshot

Convert simulation results back to snapshot format:

```csharp
// After running simulation...
var outputSnapshot = SnapshotConverter.ToSnapshot(
    result.World,
    SnapshotStatus.Completed,
    currentStep: result.CurrentStep);

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
- **Events**: Optional simulation events for dynamic scenarios
- **Steps**: Optional simulation steps for reproducibility

See [examples/example-snapshot-v4.xml](examples/example-snapshot-v4.xml) for a complete working example.

## Event System

The event system provides a powerful mechanism for creating dynamic simulation scenarios by modifying city factors
during runtime. Events support various triggers and effects for modeling policy changes, disasters, economic shifts, and
feedback mechanisms.

### Event Basics

Events consist of:

- **Trigger**: Defines when the event executes (one-time, periodic, continuous, or conditional)
- **Effect**: Defines what happens when the event executes (factor changes, feedback, or composite effects)

### Creating Events Programmatically

```csharp
// One-time event at specific step
var pollutionSpike = new SimulationEvent(
    displayName: "Pollution Spike",
    trigger: new StepTrigger(step: 50),
    effect: new FactorChangeEffect(
        targetFactor: pollutionFactor,
        changeValue: UnitValuePromise.Fixed(0.8),  // Set to 0.8 (high pollution)
        applicationType: EffectApplicationType.Temporary,
        duration: EffectDuration.OverSteps(20)
    ),
    description: "Industrial accident causes temporary pollution increase"
);

// Periodic event every N steps
var seasonalChange = new SimulationEvent(
    displayName: "Seasonal Economic Cycle",
    trigger: new PeriodicTrigger(startStep: 0, interval: 90),
    effect: new FactorChangeEffect(
        targetFactor: incomeFactor,
        changeValue: UnitValuePromise.InRange(0.9, 1.0),
        applicationType: EffectApplicationType.Multiply,
        duration: EffectDuration.Instant()
    )
);

// Continuous event active during a time window
var infrastructureUpgrade = new SimulationEvent(
    displayName: "Transport Infrastructure Upgrade",
    trigger: new ContinuousTrigger(startStep: 100, endStep: 200),
    effect: new FactorChangeEffect(
        targetFactor: transportFactor,
        changeValue: UnitValuePromise.Fixed(0.95),  // High transport quality
        applicationType: EffectApplicationType.Permanent
    ),
    description: "Gradual transport quality improvement"
);

// Composite effect modifying multiple factors
var economicBoom = new SimulationEvent(
    displayName: "Economic Boom",
    trigger: new StepTrigger(step: 150),
    effect: new CompositeEffect([
        new FactorChangeEffect(
            targetFactor: incomeFactor,
            changeValue: UnitValuePromise.Fixed(0.85),  // High income
            applicationType: EffectApplicationType.Permanent
        ),
        new FactorChangeEffect(
            targetFactor: housingCostFactor,
            changeValue: UnitValuePromise.Fixed(0.75),  // High housing cost
            applicationType: EffectApplicationType.Permanent
        )
    ])
);
```

### Application Types

Events can apply changes in different ways:

```csharp
// Absolute: Set factor to specific value
EffectApplicationType.Permanent  // Immediate permanent change
EffectApplicationType.Temporary  // Temporary change (requires duration)

// Delta: Add/subtract from current value
EffectApplicationType.Delta

// Multiply: Scale current value
EffectApplicationType.Multiply

// Transitions: Gradual changes
EffectApplicationType.LinearTransition    // Linear interpolation
EffectApplicationType.LogarithmicTransition  // Logarithmic curve
```

### Effect Duration

```csharp
// Instant change
EffectDuration.Instant()

// Change applied over N steps
EffectDuration.OverSteps(20)
```

### Adding Events to Simulation

```csharp
// Add events to world before running simulation
world.Events.Add(pollutionSpike);
world.Events.Add(seasonalChange);
world.Events.Add(infrastructureUpgrade);

// Or configure in SimulationBuilder
var engine = SimulationBuilder.Create()
    .WithConsoleOutput()
    .AddEventStage()  // Add event processing stage
    .ConfigureSimulation(s => s.MaxSteps(500))
    .Build();

var result = await engine.RunAsync(world);
```

### Events in Snapshots

Events are fully supported in XML snapshots:

```xml

<Events>
    <Event Id="pollution_spike" Name="Pollution Spike Event"
           Description="Temporary pollution increase" Completed="false">
        <Trigger Type="StepTrigger" Step="50"/>
        <Effects>
            <Effect Type="FactorChangeEffect" FactorId="pollution"
                    ApplicationType="Temporary" Duration="20">
                <ValueSpec V="10.5"/>
            </Effect>
        </Effects>
    </Event>

    <Event Id="economic_boom" Name="Economic Boom" Completed="false">
        <Trigger Type="StepTrigger" Step="150"/>
        <Effects>
            <Effect Type="CompositeEffect">
                <CompositeEffects>
                    <Effect Type="FactorChangeEffect" FactorId="income"
                            ApplicationType="Permanent">
                        <ValueSpec V="1050"/>
                    </Effect>
                    <Effect Type="FactorChangeEffect" FactorId="housing_cost"
                            ApplicationType="Permanent">
                        <ValueSpec V="4000"/>
                    </Effect>
                </CompositeEffects>
            </Effect>
        </Effects>
    </Event>
</Events>
```

### Conditional Events

For advanced scenarios, implement `IEventTrigger` with custom conditions:

```csharp
public class PopulationThresholdTrigger : IEventTrigger
{
    private readonly int _threshold;
    private bool _triggered;

    public PopulationThresholdTrigger(int threshold) => _threshold = threshold;

    public bool ShouldTrigger(SimulationContext context)
    {
        if (_triggered) return false;
        
        var totalPop = context.World.Population;
        if (totalPop > _threshold)
        {
            _triggered = true;
            return true;
        }
        return false;
    }
}
```

### Event System Benefits

- **Dynamic Scenarios**: Model real-world events affecting migration patterns
- **Flexible Triggers**: One-time, periodic, continuous, or conditional execution
- **Multiple Effect Types**: Permanent, temporary, delta, multiply, and transition effects
- **Composable**: Combine multiple effects in a single event
- **Snapshot Support**: Events are serializable and reproducible
- **Extensible**: Implement custom triggers and effects for specialized scenarios

## Configuring Simulation Parameters

You can configure the simulation execution and model parameters using modern C# syntax:

```csharp
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
    MaxSteps = 500,
    CheckStability = true,
    StabilityThreshold = 100,  // Consider stable if <100 persons migrate
    StabilityCheckInterval = 5,
    MinStepsBeforeStabilityCheck = 20
};

// Create simulation engine with custom configuration using builder (recommended)
var engine = SimulationBuilder.Create()
    .ConfigureModel(m => 
    {
        m.CapacitySteepness = 5.0;
        m.DistanceDecayLambda = 0.001;
        m.MigrationProbabilitySteepness = 10.0;
        m.MigrationProbabilityThreshold = 0.0;
        m.FactorSmoothingAlpha = 0.2;
    })
    .ConfigureSimulation(s => 
    {
        s.MaxSteps = 500;
        s.CheckStability = true;
        s.StabilityThreshold = 100;
        s.StabilityCheckInterval = 5;
        s.MinStepsBeforeStabilityCheck = 20;
    })
    .WithConsoleOutput()
    .Build();

// Run simulation
var result = await engine.RunAsync(world);
```

## Simulation Metrics

The framework provides metrics collection for academic analysis:

```csharp
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
Console.WriteLine($"Total steps: {collector.History.Count}");
Console.WriteLine($"Average migration rate: {collector.AverageMigrationRate:P2}");
Console.WriteLine($"Total migrations: {collector.TotalMigrations:N0}");

// Access per-step statistics
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

**Per-step metrics:**

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

## Performance Optimization

For optimal performance in simulations, factor intensities are stored as UnitValue (0-1 range):

```csharp
// After setting up your world with UnitValue factor intensities
var city = new City(
    factorIntensities: [
        new FactorIntensity { Definition = incomeFactor, Value = UnitValue.FromRatio(0.5) }
    ],
    persons: collection.GenerateAllPersons(allFactors));

// Run your simulation
for (int step = 0; step < 100; step++)
{
    // Simulation logic here
}
```

## Note on Snapshots

- Snapshots include a Version field set to "beta" for informational purposes
- During beta development, version validation is temporarily disabled
- Snapshots are forward and backward compatible within the beta period