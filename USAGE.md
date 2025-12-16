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
collection.Add(new StandardPersonGenerator
{
    Count = 100000,
    // Use FactorDefinition references for type safety
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

Set up the simulation using the fluent builder API (recommended approach).

```csharp
using dotMigrata.Simulation.Builders;

var engine = SimulationBuilder.Create()
    .WithConsoleOutput()
    .ConfigureSimulation(s => s.MaxTicks(100))
    .Build();
```

Alternatively, for advanced scenarios with custom calculators:

```csharp
using dotMigrata.Simulation.Pipeline;

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

Console.WriteLine($"Simulation completed after {result.CurrentTick} ticks");
Console.WriteLine($"Final population: {result.World.Population:N0} persons");

// Dispose the engine to release resources
await engine.DisposeAsync();
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
collection.Add(new StandardPersonGenerator(seed: 42)
{
    Count = 100_000,
    // Use FactorDefinition references for type safety
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
- Type-safe FactorDefinition references

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
collection.Add(new StandardPersonGenerator(seed: 42)
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
/// <remarks>
/// Note: With World's person type validation, all persons will be DemographicPerson
/// when using this calculator, so pattern matching is safe.
/// </remarks>
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

        // Since World validates all persons are the same type, we can safely cast
        var demoPerson = (DemographicPerson)person;
        
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
- **Implement `IPersonGenerator<TPerson>`**: Create a custom generator for your person type
- **Single type per world**: The framework enforces that all persons in a simulation are the same type
- **Thread safety**: Ensure custom properties are immutable (use `init` instead of `set`)
- **Tags support**: All person types inherit `Tags` property from `PersonBase` for consistent categorization
- **Factory pattern**: Consider creating factory methods for complex initialization

### Generating Custom Person Types

Version `0.5.2-beta` provides a type-safe way to generate custom person types by implementing the
`IPersonGenerator<TPerson>` interface. This approach is cleaner and more flexible than the deprecated PersonFactory
pattern.

```csharp
using dotMigrata.Core.Entities;
using dotMigrata.Core.Enums;
using dotMigrata.Core.Values;
using dotMigrata.Generator;

// Define your custom person type
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

// Implement a custom generator for your person type
public sealed class DemographicPersonGenerator : IPersonGenerator<DemographicPerson>
{
    private readonly Random _random;

    public DemographicPersonGenerator(int seed = 0)
    {
        _random = seed == 0 ? new Random() : new Random(seed);
    }

    public required int Count { get; init; }
    public required ValueSpecification MovingWillingness { get; init; }
    public required ValueSpecification RetentionRate { get; init; }
    public required ValueSpecification Age { get; init; }
    public required ValueSpecification Income { get; init; }
    public Dictionary<FactorDefinition, ValueSpecification> FactorSensitivities { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];

    public IEnumerable<DemographicPerson> Generate(IEnumerable<FactorDefinition> factorDefinitions)
    {
        var factors = factorDefinitions.ToList();
        for (var i = 0; i < Count; i++)
        {
            // Generate base properties
            var sensitivities = new Dictionary<FactorDefinition, double>();
            foreach (var factor in factors)
            {
                if (FactorSensitivities.TryGetValue(factor, out var spec))
                    sensitivities[factor] = GenerateValue(spec);
                else
                    sensitivities[factor] = _random.NextDouble() * 10 - 5; // Default range
            }

            var age = (int)GenerateValue(Age);
            var income = GenerateValue(Income);
            var educationLevel = age < 25 ? "HighSchool" : 
                               age < 30 ? "Bachelor" :
                               age < 40 ? "Master" : "PhD";

            yield return new DemographicPerson(sensitivities)
            {
                MovingWillingness = NormalizedValue.FromRatio(GenerateValue(MovingWillingness)),
                RetentionRate = NormalizedValue.FromRatio(GenerateValue(RetentionRate)),
                Age = age,
                Income = income,
                EducationLevel = educationLevel,
                IsEmployed = _random.NextDouble() > 0.1,
                Tags = Tags.ToList()
            };
        }
    }

    private double GenerateValue(ValueSpecification spec)
    {
        if (spec.IsFixed) return spec.FixedValue!.Value;
        if (spec.HasRange)
        {
            var (min, max) = spec.Range!.Value;
            return min + _random.NextDouble() * (max - min);
        }
        return _random.NextDouble();
    }
}

// Usage
var incomeFactor = new FactorDefinition
{
    DisplayName = "Income",
    Type = FactorType.Positive,
    MinValue = 20000,
    MaxValue = 100000
};

FactorDefinition[] allFactors = [incomeFactor];

var collection = new PersonCollection();
collection.Add(new DemographicPersonGenerator(seed: 42)
{
    Count = 10000,
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [incomeFactor] = ValueSpecification.InRange(3, 8)
    },
    MovingWillingness = ValueSpecification.InRange(0.4, 0.7),
    RetentionRate = ValueSpecification.InRange(0.3, 0.6),
    Age = ValueSpecification.InRange(18, 65),
    Income = ValueSpecification.InRange(25000, 120000),
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
- **Clean API**: No unwieldy 7-parameter functions
- **Testable**: Easy to unit test generator logic
- **Discoverable**: Clear interface makes implementation obvious

### Custom Person Types and Snapshots

The snapshot system fully supports custom person types through the type discriminator pattern.

### Using Custom Person Types with Snapshots

**Version 0.6.4+** introduces complete support for custom person types in snapshots. You can now:

- Save and load snapshots with custom person types
- Implement custom serialization for additional properties
- Use type-safe deserialization with the `PersonTypeRegistry`

#### Default Behavior

Without registration, snapshots default to `StandardPerson`:

```csharp
using dotMigrata.Snapshot.Serialization;
using dotMigrata.Snapshot.Conversion;

// Converting a world to snapshot
var snapshot = SnapshotConverter.ToSnapshot(world);
XmlSnapshotSerializer.SerializeToFile(snapshot, "world-snapshot.xml");

// Loading back - creates StandardPerson instances by default
var loadedSnapshot = XmlSnapshotSerializer.DeserializeFromFile("world-snapshot.xml");
var loadedWorld = SnapshotConverter.ToWorld(loadedSnapshot);
```

#### Registering Custom Person Types

To support custom person types in snapshots, implement the serializer interfaces and register:

```csharp
using dotMigrata.Snapshot.Conversion;
using dotMigrata.Core.Entities;
using dotMigrata.Core.Values;
using System.Xml;

// 1. Create a custom person serializer
public class DemographicPersonSerializer : ICustomPersonSerializer<DemographicPerson>
{
    public DemographicPerson CreateFromTemplate(
        PersonTemplateXml template,
        Dictionary<FactorDefinition, double> sensitivities,
        List<string> tags)
    {
        // Extract custom properties from XML
        int age = 30; // default
        double income = 50000; // default
        string education = "Bachelor";

        if (template.CustomProperties != null)
        {
            var ageNode = template.CustomProperties.SelectSingleNode("Age");
            if (ageNode != null) age = int.Parse(ageNode.InnerText);

            var incomeNode = template.CustomProperties.SelectSingleNode("Income");
            if (incomeNode != null) income = double.Parse(incomeNode.InnerText);

            var eduNode = template.CustomProperties.SelectSingleNode("Education");
            if (eduNode != null) education = eduNode.InnerText;
        }

        return new DemographicPerson(sensitivities)
        {
            MovingWillingness = NormalizedValue.FromRatio(template.MovingWillingness),
            RetentionRate = NormalizedValue.FromRatio(template.RetentionRate),
            Age = age,
            Income = income,
            EducationLevel = education,
            Tags = tags
        };
    }

    public XmlElement? SerializeCustomProperties(DemographicPerson person, XmlDocument doc)
    {
        var customProps = doc.CreateElement("CustomProperties");

        var ageElem = doc.CreateElement("Age");
        ageElem.InnerText = person.Age.ToString();
        customProps.AppendChild(ageElem);

        var incomeElem = doc.CreateElement("Income");
        incomeElem.InnerText = person.Income.ToString();
        customProps.AppendChild(incomeElem);

        var eduElem = doc.CreateElement("Education");
        eduElem.InnerText = person.EducationLevel;
        customProps.AppendChild(eduElem);

        return customProps;
    }
}

// 2. Register the custom person type at application startup
PersonTypeRegistry.RegisterPersonType("DemographicPerson", new DemographicPersonSerializer());

// 3. Now snapshots will correctly serialize/deserialize DemographicPerson
var snapshot = SnapshotConverter.ToSnapshot(world);
XmlSnapshotSerializer.SerializeToFile(snapshot, "demographic-world.xml");

var loadedSnapshot = XmlSnapshotSerializer.DeserializeFromFile("demographic-world.xml");
var loadedWorld = SnapshotConverter.ToWorld(loadedSnapshot); // Returns DemographicPerson instances!
```

#### Custom Person Type in XML

The XML snapshot will include the `PersonType` attribute and custom properties:

```xml

<Person Count="1000" PersonType="DemographicPerson" Willingness="0.7" Retention="0.4">
    <Sensitivities>
        <S Id="income" Value="8.5"/>
        <S Id="pollution" Value="-6.0"/>
    </Sensitivities>
    <CustomProperties>
        <Age>28</Age>
        <Income>75000</Income>
        <Education>Master</Education>
    </CustomProperties>
</Person>
```

#### Using Custom Generators with Snapshots

For generator-based populations, implement `ICustomGeneratorSerializer`:

```csharp
public class DemographicGeneratorSerializer : 
    ICustomGeneratorSerializer<DemographicPerson, DemographicPersonGenerator>
{
    public DemographicPersonGenerator CreateFromXml(
        GeneratorXml generatorXml,
        Dictionary<FactorDefinition, ValueSpecification> factorSpecs,
        List<string> tags)
    {
        // Create generator with custom specifications
        return new DemographicPersonGenerator(generatorXml.Seed)
        {
            Count = generatorXml.Count,
            FactorSensitivities = factorSpecs,
            // ... set custom generator properties from generatorXml.CustomProperties
            Tags = tags
        };
    }

    public XmlElement? SerializeCustomProperties(
        DemographicPersonGenerator generator, 
        XmlDocument doc)
    {
        // Serialize custom generator specs if any
        return null; // or return custom XML element
    }
}

// Register generator serializer
PersonTypeRegistry.RegisterGeneratorType<DemographicPerson, DemographicPersonGenerator>(
    "DemographicPerson", 
    new DemographicGeneratorSerializer());
```

### Key Points

- **Register once**: Register custom types at application startup before loading/saving snapshots
- **Type names**: Use consistent type names (e.g., "DemographicPerson") across registration and XML
- **Backward compatible**: Existing snapshots without `PersonType` attribute default to "StandardPerson"
- **Clean separation**: CustomProperties keeps core schema simple while allowing extensibility

## Configuring Simulation Parameters

You can configure the simulation execution and model parameters using modern C# syntax:

```csharp
using dotMigrata.Logic.Calculators;
using dotMigrata.Logic.Models;
using dotMigrata.Simulation.Builders;
using dotMigrata.Simulation.Models;

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
        s.MaxTicks = 500;
        s.CheckStability = true;
        s.StabilityThreshold = 100;
        s.StabilityCheckInterval = 5;
        s.MinTicksBeforeStabilityCheck = 20;
    })
    .WithConsoleOutput()
    .Build();

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