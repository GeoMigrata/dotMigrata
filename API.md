# API Reference

This document provides a comprehensive reference for the public API of dotMigrata.

## Main Entry Points

### World

Top-level entity containing cities and factor definitions.

**Constructor:**

```csharp
new World(
    IEnumerable<City> cities,
    IEnumerable<FactorDefinition> factorDefinitions)
{ DisplayName = "..." }
```

**Properties:**

- `DisplayName` (string) - Display name of the world
- `Cities` (IReadOnlyList<City>) - List of cities
- `FactorDefinitions` (IReadOnlyList<FactorDefinition>) - List of factor definitions
- `AllPersons` (IEnumerable<PersonBase>) - All persons across all cities
- `Population` (int) - Total population across all cities

**Example:**

```csharp
var world = new World([city1, city2], [factor1, factor2])
{
    DisplayName = "My World"
};
```

### City

Represents a city with geographic location, factors, and individual persons.

**Properties:**

- `DisplayName` (string, required) - Display name
- `Area` (double, init) - Area in square kilometers (must be > 0)
- `Location` (Coordinate, required) - Geographic coordinates
- `Capacity` (int?, init) - Maximum population capacity (optional, null or 0 means no limit)
- `FactorIntensities` (IReadOnlyList<FactorIntensity>) - Factor intensities for this city
- `Persons` (IReadOnlyList<PersonBase>) - Persons residing in this city (returns snapshot)
- `Population` (int) - Total population (count of persons)

**Methods:**

- `UpdateFactorIntensity(FactorDefinition, ValueSpec)` - Updates a factor's intensity
- `TryGetFactorIntensity(FactorDefinition, out FactorIntensity?)` - Gets a factor intensity
- `AddPerson(PersonBase)` - Adds a person to this city (thread-safe)
- `RemovePerson(PersonBase)` - Removes a person from this city (thread-safe, returns bool)

### PersonBase (Abstract)

Abstract base class for all person types in the simulation. Defines core migration-essential properties that all person
types must implement.

**Properties:**

- `CurrentCity` (City?) - Current city where the person resides (mutable for migration)
- `MovingWillingness` (NormalizedValue) - Willingness to migrate (0-1 range, required)
- `RetentionRate` (NormalizedValue) - Tendency to stay in current location (0-1 range, required)
- `FactorSensitivities` (IReadOnlyDictionary<FactorDefinition, double>) - Factor sensitivities
- `Tags` (IReadOnlyList<string>) - Tags for categorization and statistical analysis

**Constructor:**

```csharp
protected PersonBase(IDictionary<FactorDefinition, double> factorSensitivities)
```

**Methods:**

- `GetSensitivity(FactorDefinition)` - Gets sensitivity value for a factor (returns SensitivityValue)
- `UpdateSensitivity(FactorDefinition, SensitivityValue)` - Updates sensitivity for a factor

**Remarks:**

Custom person types can inherit from `PersonBase` to add domain-specific properties. The framework guarantees all person
types have the essential properties needed for migration logic. All person types support tags for consistent
categorization.

### StandardPerson

Standard implementation of a person with properties for the default migration model. This is the concrete class that
replaces the old `Person` class.

**Inheritance:** `PersonBase`

**Properties:**

- Inherits all properties from `PersonBase` (`CurrentCity`, `MovingWillingness`, `RetentionRate`, `FactorSensitivities`,
  `Tags`)
- `SensitivityScaling` (double, init) - Attraction scaling coefficient (default: 1.0)
- `AttractionThreshold` (double, init) - Minimum attraction difference for migration (default: 0.0)
- `MinimumAcceptableAttraction` (double, init) - Minimum destination attraction (default: 0.0)

**Constructor:**

```csharp
new StandardPerson(IDictionary<FactorDefinition, double> factorSensitivities)
{
    MovingWillingness = NormalizedValue.FromRatio(0.5),
    RetentionRate = NormalizedValue.FromRatio(0.3),
    SensitivityScaling = 1.0,
    AttractionThreshold = 0.0,
    MinimumAcceptableAttraction = 0.0,
    Tags = ["example_tag"]
}
```

**Methods:**

- Inherits `GetSensitivity(FactorDefinition)` and `UpdateSensitivity(FactorDefinition, SensitivityValue)` from
  `PersonBase`

**Remarks:**

Used by `StandardAttractionCalculator` (which uses pattern matching to access `SensitivityScaling`) and
`StandardMigrationCalculator` (which uses pattern matching to access `AttractionThreshold` and
`MinimumAcceptableAttraction`). For custom migration models, create a new class inheriting from `PersonBase` instead of
modifying this class.

**Example of Pattern Matching in StandardAttractionCalculator:**

```csharp
// Apply person's sensitivity scaling if it's a StandardPerson
if (person is StandardPerson stdPerson)
    totalScore *= stdPerson.SensitivityScaling;
```

### Creating Custom Person Types

To create custom person types, inherit from `PersonBase`:

```csharp
public sealed class CustomPerson : PersonBase
{
    public CustomPerson(IDictionary<FactorDefinition, double> factorSensitivities)
        : base(factorSensitivities)
    {
    }

    // Add custom properties
    public int Age { get; init; }
    public double Income { get; init; }
}
```

Then use pattern matching in custom calculators to access the additional properties:

```csharp
if (person is CustomPerson customPerson)
{
    // Access custom properties
    var ageAdjustment = customPerson.Age < 30 ? 1.2 : 1.0;
    // ... custom logic
}
```

### FactorDefinition

Defines a city characteristic that influences migration.

**Properties:**

- `DisplayName` (string, required) - Human-readable name
- `Type` (FactorType, required) - Direction (Positive or Negative)
- `TransformFunction` (UnitValuePromise.TransformFunc?, optional) - Optional transformation function

**Note:** Factor intensities are stored as normalized `UnitValue` (0-1 range), so `MinValue` and
`MaxValue` are no longer needed. Factor values must be pre-normalized before creating `FactorIntensity`.

**Example:**

```csharp
var incomeFactor = new FactorDefinition
{
    DisplayName = "Average Income",
    Type = FactorType.Positive,
    TransformFunction = null  // Optional transformation
};
```

### FactorIntensity

Represents the intensity value for a specific factor in a city.

**Properties:**

- `Definition` (FactorDefinition, required) - The factor definition
- `Value` (UnitValue, required) - The normalized intensity value (0-1 range)

**Example:**

```csharp
var intensity = new FactorIntensity
{
    Definition = incomeFactor,
    Value = UnitValue.FromRatio(0.6)  // Normalized 0-1 value
};
```

### Coordinate

Geographic coordinate (WGS84).

**Properties:**

- `Latitude` (double, -90 to 90) - Latitude in degrees
- `Longitude` (double, -180 to 180) - Longitude in degrees

**Methods:**

- `DistanceTo(Coordinate)` - Calculate distance in kilometers
- `static DistanceBetween(Coordinate, Coordinate)` - Calculate distance between two coordinates

## Generator

### PersonCollection

A collection of person specifications that can be used to generate a population.

**Properties:**

- `SpecificationCount` (int) - Number of specifications in the collection

**Methods:**

```csharp
PersonCollection Add(PersonBase person)
PersonCollection Add(PersonBase person, int count)
PersonCollection Add<TPerson>(IPersonGenerator<TPerson> generator) where TPerson : PersonBase
IEnumerable<PersonBase> GenerateAllPersons(IEnumerable<FactorDefinition> factorDefinitions)
int GetTotalCount()
void Clear()
```

### IPersonGenerator<TPerson>

Generic interface for implementing custom person generators. Define your own generator to create custom person types
with specific properties and generation logic.

**Properties:**

- `Count` (int) - Number of persons to generate

**Methods:**

```csharp
IEnumerable<TPerson> Generate(IEnumerable<FactorDefinition> factorDefinitions)
```

### StandardPersonGenerator

Concrete implementation of `IPersonGenerator<StandardPerson>` for generating StandardPerson instances with randomized
or specified attributes.

**Constructors:**

```csharp
new StandardPersonGenerator() // Random seed
new StandardPersonGenerator(int seed) // Specific seed for reproducibility
```

**Properties:**

- `Count` (int) - Number of persons to generate - Required
- `FactorSensitivities` (Dictionary<FactorDefinition, ValueSpec>) - Factor sensitivity specifications
- `MovingWillingness` (ValueSpec) - Willingness specification - Required
- `RetentionRate` (ValueSpec) - Retention specification - Required
- `SensitivityScaling` (ValueSpec?) - Scaling specification - Optional (default: 1.0)
- `AttractionThreshold` (ValueSpec?) - Threshold specification - Optional (default: 0.0)
- `MinimumAcceptableAttraction` (ValueSpec?) - Min attraction specification - Optional (default: 0.0)
- `Tags` (IReadOnlyList<string>) - Tags to assign to all generated persons - Optional
- `DefaultSensitivityRange` (ValueRange) - Default range for unspecified factors - Default: (-10.0, 10.0)
- `SensitivityStdDev` (double) - Standard deviation for sensitivity normal distribution - Default: 3.0

**Methods:**

```csharp
IEnumerable<StandardPerson> Generate(IEnumerable<FactorDefinition> factorDefinitions)
```

### ValueSpec

Represents a value specification for attributes. Can be a fixed value, a random range, or an approximate value using
normal distribution.

**Static Factory Methods:**

```csharp
static ValueSpec Fixed(double value)
static ValueSpec InRange(double min, double max)
static ValueSpec Approximately(double mean, double standardDeviation)
static ValueSpec Random()
static ValueSpec RandomWithScale(double scale)
```

**Methods:**

```csharp
ValueSpec WithScale(double scale)
```

**Properties:**

- `IsFixed` (bool) - Whether this is a fixed value
- `HasRange` (bool) - Whether this has a custom range
- `IsApproximate` (bool) - Whether this is an approximate value using normal distribution
- `FixedValue` (double?) - The fixed value if applicable
- `Range` ((double Min, double Max)?) - The range if applicable
- `Mean` (double?) - The mean value for approximate specifications
- `StandardDeviation` (double?) - The standard deviation for approximate specifications
- `Scale` (double) - The scale factor

**Examples:**

```csharp
// Fixed value - all persons get 5.0
ValueSpec.Fixed(5.0)

// Random in range 0.3 to 0.8 (uniform distribution)
ValueSpec.InRange(0.3, 0.8)

// Approximate value using normal distribution
ValueSpec.Approximately(mean: 0.5, standardDeviation: 0.1)

// Random with default range
ValueSpec.Random()

// Random with default range, scaled by 1.5 (bias higher)
ValueSpec.Random().WithScale(1.5)

// Random in range, scaled by 0.5 (bias lower)
ValueSpec.InRange(0.2, 0.8).WithScale(0.5)
```

## Configuration

### SimulationConfig

Controls simulation execution.

**Properties:**

- `MaxSteps` (int) - Maximum simulation steps (default: 1000)
- `CheckStability` (bool) - Whether to check for stability (default: true)
- `StabilityThreshold` (int) - Population change threshold for stability (default: 10)
- `StabilityCheckInterval` (int) - How often to check stability (default: 1)
- `MinStepsBeforeStabilityCheck` (int) - Minimum steps before checking (default: 10)

**Static:**

- `Default` - Default configuration instance

### StandardModelConfig

Configures the standard model's mathematical parameters.

**Properties:**

- `CapacitySteepness` (double) - Capacity resistance steepness (default: 5.0)
- `DistanceDecayLambda` (double) - Distance decay coefficient (default: 0.001)
- `MigrationProbabilitySteepness` (double) - Migration probability steepness (default: 10.0)
- `MigrationProbabilityThreshold` (double) - Migration threshold (default: 0.0)
- `FactorSmoothingAlpha` (double) - Factor update smoothing (default: 0.2)

**Static:**

- `Default` - Default configuration instance

## Interfaces

### IAttractionCalculator

Interface for calculating city attraction for individual persons.

**Methods:**

```csharp
AttractionResult CalculateAttraction(
    City city, 
    PersonBase person, 
    City? originCity = null)

IDictionary<City, AttractionResult> CalculateAttractionForAllCities(
    IEnumerable<City> cities, 
    PersonBase person, 
    City? originCity = null)
```

**Implementations:**

- `StandardAttractionCalculator` - Standard model implementation (uses pattern matching for `StandardPerson` properties)

### IMigrationCalculator

Interface for calculating individual migration decisions.

**Methods:**

```csharp
MigrationFlow? CalculateMigrationDecision(
    PersonBase person,
    IEnumerable<City> destinationCities,
    IDictionary<City, AttractionResult> attractionResults)

IEnumerable<MigrationFlow> CalculateAllMigrationFlows(
    World world,
    IAttractionCalculator attractionCalculator)
```

**Implementations:**

- `StandardMigrationCalculator` - Standard model implementation with parallel processing

### ISimulationStage

Represents a stage in the simulation pipeline.

**Properties:**

- `Name` (string) - Stage name

**Methods:**

```csharp
Task ExecuteAsync(SimulationContext context)
bool ShouldExecute(SimulationContext context) // Default: true
```

**Built-in Stages:**

- `MigrationDecisionStage` - Calculates individual migration decisions
- `MigrationExecutionStage` - Executes migrations by moving persons

### ISimulationObserver

Observer pattern for monitoring simulation.

**Methods:**

```csharp
void OnSimulationStart(SimulationContext context)
void OnStepStart(SimulationContext context)
void OnStageComplete(string stageName, SimulationContext context)
void OnStepComplete(SimulationContext context)
void OnSimulationEnd(SimulationContext context, string reason)
void OnError(SimulationContext context, Exception exception)
```

**Implementations:**

- `ConsoleObserver` - Prints to console with person-based statistics

## Simulation Pipeline

### Creating a Simulation

To create and run a simulation, the recommended approach is to use the `SimulationBuilder`:

```csharp
using dotMigrata.Simulation.Builders;

// Create and configure simulation using the fluent builder API
var engine = SimulationBuilder.Create()
    .WithConsoleOutput()
    .ConfigureSimulation(s => s.MaxSteps(100))
    .Build();

// Run simulation
var result = await engine.RunAsync(world);

// Release resources
await engine.DisposeAsync();
```

For advanced scenarios with custom calculators and stages:

```csharp
using dotMigrata.Logic.Calculators;
using dotMigrata.Simulation.Engine;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Pipeline;
using dotMigrata.Simulation.Models;

// Create calculators
var attractionCalc = new StandardAttractionCalculator();
using var migrationCalc = new StandardMigrationCalculator();

// Create pipeline stages
var stages = new List<ISimulationStage>
{
    new MigrationDecisionStage(migrationCalc, attractionCalc),
    new MigrationExecutionStage()
};

// Create simulation engine
var config = SimulationConfig.Default;
var engine = new SimulationEngine(stages, config);

// Add observers
engine.AddObserver(new ConsoleObserver(colored: true));

// Run simulation
var result = await engine.RunAsync(world);

// Release resources
await engine.DisposeAsync();
```

### Standard Pipeline Stages

**MigrationDecisionStage** - Calculates migration decisions for all persons
**MigrationExecutionStage** - Executes migrations by moving persons between cities

## Simulation Engine

### SimulationEngine

Main simulation orchestrator.

**Constructor:**

```csharp
new SimulationEngine(
    IEnumerable<ISimulationStage> stages,
    SimulationConfig? config = null)
```

**Methods:**

- `AddObserver(ISimulationObserver)` - Adds an observer
- `RemoveObserver(ISimulationObserver)` - Removes an observer
- `Task<SimulationContext> RunAsync(World world)` - Runs the simulation

### SimulationContext

Runtime simulation state shared between stages.

**Properties:**

- `World` (World) - The world being simulated
- `CurrentStep` (int) - Current step number
- `IsStabilized` (bool) - Whether the simulation has stabilized
- `TotalPopulationChange` (int) - Total population change in current step
- `MaxCityPopulationChange` (int) - Maximum city population change
- `CurrentMigrationFlows` (IEnumerable<MigrationFlow>) - Current migration flows

**Methods:**

- `SetData<T>(string key, T value)` - Stores data for inter-stage communication
- `T? GetData<T>(string key)` - Retrieves stored data
- `bool TryGetData<T>(string key, out T? value)` - Tries to retrieve data

## Snapshot System

The snapshot system uses PersonCollection-based architecture for efficient, deterministic simulation state management.
Person properties are immutable (except CurrentCity), enabling PersonCollections to be stored as specifications rather
than individual instances.

### XmlSnapshotSerializer

Attribute-based XML serialization using `System.Xml.Serialization` with namespace support for code concepts vs. snapshot
containers.

**Methods:**

```csharp
static string Serialize(WorldSnapshotXml snapshot)
static void SerializeToFile(WorldSnapshotXml snapshot, string filePath)

static WorldSnapshotXml? Deserialize(string xml)
static WorldSnapshotXml? DeserializeFromFile(string filePath)
```

**Features:**

- **Namespace Design**: Distinguishes code concepts (`c:`) from snapshot containers (default namespace)
- **PersonCollection Storage**: Stores collection specifications (templates + generators) instead of individual persons
- **Deterministic Reproducibility**: Uses random seeds to regenerate exact simulation states from step count
- **Attribute-Based Format**: Simple properties as attributes, complex structures as elements
- **Efficient**: Supports millions of persons without storing individual instances
- **Immutable Person Properties**: StandardPerson instances regenerated from specifications on load

**XML Structure with Namespaces:**

```xml

<Snapshot xmlns="https://geomigrata.pages.dev/snapshot"
          xmlns:c="https://geomigrata.pages.dev/code"
          Version="v4" Id="..." Status="Seed" CreatedAt="..." CurrentStep="0">
    <World DisplayName="Example World">
        <!-- FactorDefinitions is a snapshot container (default namespace) -->
        <FactorDefinitions>
            <!-- c:FactorDefinition maps to FactorDefinition class in code -->
            <c:FactorDefinition Id="pollution" DisplayName="Pollution" Type="Negative" Min="0" Max="12"
                                CustomTransformName="Linear"/>
        </FactorDefinitions>

        <!-- PersonCollections are snapshot-only constructs -->
        <PersonCollections>
            <PersonCollection Id="young_people">
                <!-- c:StandardPerson maps to StandardPerson class with Count attribute for duplication -->
                <c:StandardPerson Count="1" MovingWillingness="0.5" RetentionRate="0.3"
                                  AttractionThreshold="0.3" SensitivityScaling="0.5"
                                  MinimumAcceptableAttraction="10" Tags="tag1;tag2">
                    <FactorSensitivities>
                        <Sensitivity Id="pollution" Value="0.5"/>
                    </FactorSensitivities>
                </c:StandardPerson>

                <!-- Generator uses ValueSpecs for mass person generation -->
                <Generator Count="100000">
                    <Seed>42</Seed>
                    <FactorSensitivities>
                        <Sensitivity Id="pollution">
                            <InRange Min="-7" Max="-3"/>
                        </Sensitivity>
                    </FactorSensitivities>
                    <MovingWillingness>
                        <InRange Min="0.5" Max="1.0"/>
                    </MovingWillingness>
                    <RetentionRate>
                        <Fixed Value="0.3"/>
                    </RetentionRate>
                </Generator>
            </PersonCollection>
        </PersonCollections>

        <!-- Cities container (default namespace) -->
        <Cities>
            <!-- c:City maps to City class in code -->
            <c:City Id="shanghai" DisplayName="Shanghai" Latitude="31.2304" Longitude="121.4737" Area="6340.5">
                <FactorValues>
                    <FactorValue Id="pollution" Value="8"/>
                </FactorValues>
                <PersonCollections>
                    <CollectionRef Id="young_people"/>
                </PersonCollections>
            </c:City>
        </Cities>
    </World>

    <!-- Steps for simulation reproducibility -->
    <Steps/>
</Snapshot>
```

**ValueSpec Types:**

- `<Fixed Value="0.5"/>` - Fixed value for all generated persons
- `<InRange Min="0.3" Max="0.8"/>` - Random value within range
- `<Random/>` or `<Random Scale="1.5"/>` - Random with optional scaling

**Shorthand for Fixed Values:**

```xml
<!-- Full form -->
<MovingWillingness>
    <Fixed Value="0.5"/>
</MovingWillingness>

        <!-- Shorthand -->
<MovingWillingness Value="0.5"/>
```

**Architecture Benefits:**

1. **PersonCollections are permanent** - Like FactorDefinitions, they don't change during simulation
2. **Deterministic reproduction** - Same seed + step count = exact same state
3. **Scalable** - Millions of persons without storing individual instances
4. **Immutable persons** - Only CurrentCity changes, all other properties from specifications
5. **Collections "expand" at startup** - Generators run to produce actual PersonBase/StandardPerson instances

### SnapshotStatus

```csharp
public enum SnapshotStatus
{
    Seed,       // Initial snapshot with no simulation steps performed
    Active,     // Active snapshot that can continue simulation
    Stabilized, // Simulation has stabilized and should not continue
    Completed   // Maximum iterations reached
}
```

## Result Models

### AttractionResult

Result of attraction calculation for an individual person.

**Properties:**

- `City` (City) - The evaluated city
- `BaseAttraction` (double, 0-1) - Base attraction from factors
- `AdjustedAttraction` (double, 0-1) - Adjusted for capacity and distance
- `CapacityResistance` (double, 0-1) - Capacity resistance factor
- `DistanceResistance` (double, 0-1) - Distance decay factor

### MigrationFlow

Represents an individual person's migration decision.

**Properties:**

- `OriginCity` (City) - Origin city
- `DestinationCity` (City) - Destination city
- `Person` (PersonBase) - The individual person migrating
- `MigrationProbability` (double, 0-1) - Migration probability

## Enums

### FactorType

- `Positive` - Increases attraction (e.g., income)
- `Negative` - Decreases attraction (e.g., pollution)

### SnapshotStatus

- `Seed` - Initial state
- `Active` - Simulation in progress
- `Stabilized` - Simulation stabilized
- `Completed` - Simulation completed

## Snapshot Conversion

### SnapshotConverter

Provides bidirectional conversion between World domain objects and XML snapshot models.

**Methods:**

```csharp
static World ToWorld(WorldSnapshotXml snapshot)
static WorldSnapshotXml ToSnapshot(World world, SnapshotStatus status = Seed, int currentStep = 0)
```

**Example:**

```csharp
using dotMigrata.Snapshot.Conversion;
using dotMigrata.Snapshot.Serialization;

// Load snapshot from file and convert to World
var snapshot = XmlSnapshotSerializer.DeserializeFromFile("snapshot.xml");
var world = SnapshotConverter.ToWorld(snapshot!);

// Run simulation
var result = await engine.RunAsync(world);

// Export to snapshot
var outputSnapshot = SnapshotConverter.ToSnapshot(result.World, SnapshotStatus.Completed, result.CurrentStep);
XmlSnapshotSerializer.SerializeToFile(outputSnapshot, "output.xml");
```

### Custom Person Types in Snapshots

The snapshot system supports custom person types through a type discriminator pattern.

#### PersonTypeRegistry

Static registry for custom person type serializers.

**Methods:**

```csharp
static void RegisterPersonType<TPerson>(string typeName, ICustomPersonSerializer<TPerson> serializer)
static void RegisterGeneratorType<TPerson, TGenerator>(string typeName, ICustomGeneratorSerializer<TPerson, TGenerator> serializer)
```

**Example:**

```csharp
using dotMigrata.Snapshot.Conversion;

// Register at application startup
PersonTypeRegistry.RegisterPersonType("DemographicPerson", new DemographicPersonSerializer());
PersonTypeRegistry.RegisterGeneratorType<DemographicPerson, DemographicPersonGenerator>(
    "DemographicPerson", new DemographicGeneratorSerializer());
```

#### ICustomPersonSerializer<TPerson>

Defines serialization for custom person types.

**Methods:**

```csharp
TPerson CreateFromTemplate(PersonTemplateXml template, Dictionary<FactorDefinition, double> sensitivities, List<string> tags)
XmlElement? SerializeCustomProperties(TPerson person, XmlDocument doc)
```

**Implementation Example:**

```csharp
public class DemographicPersonSerializer : ICustomPersonSerializer<DemographicPerson>
{
    public DemographicPerson CreateFromTemplate(
        PersonTemplateXml template,
        Dictionary<FactorDefinition, double> sensitivities,
        List<string> tags)
    {
        // Extract custom properties from template.CustomProperties
        int age = 30;
        if (template.CustomProperties != null)
        {
            var ageNode = template.CustomProperties.SelectSingleNode("Age");
            if (ageNode != null) age = int.Parse(ageNode.InnerText);
        }

        return new DemographicPerson(sensitivities)
        {
            MovingWillingness = NormalizedValue.FromRatio(template.MovingWillingness),
            RetentionRate = NormalizedValue.FromRatio(template.RetentionRate),
            Age = age,
            Tags = tags
        };
    }

    public XmlElement? SerializeCustomProperties(DemographicPerson person, XmlDocument doc)
    {
        var customProps = doc.CreateElement("CustomProperties");
        var ageElem = doc.CreateElement("Age");
        ageElem.InnerText = person.Age.ToString();
        customProps.AppendChild(ageElem);
        return customProps;
    }
}
```

#### ICustomGeneratorSerializer<TPerson, TGenerator>

Defines serialization for custom person generators.

**Methods:**

```csharp
TGenerator CreateFromXml(GeneratorXml generatorXml, Dictionary<FactorDefinition, ValueSpec> factorSpecs, List<string> tags)
XmlElement? SerializeCustomProperties(TGenerator generator, XmlDocument doc)
```

**Key Points:**

- **StandardPerson**: Registered by default, no explicit registration needed
- **Type discriminator**: `PersonType` XML attribute specifies the concrete type
- **Custom properties**: `CustomProperties` XML element contains type-specific data
- **Backward compatible**: Snapshots without `PersonType` default to "StandardPerson"
- **Thread safety**: Register types during initialization before snapshot operations

## Simulation Builders

### SimulationBuilder

Fluent builder for creating and configuring simulation engines.

**Methods:**

```csharp
static SimulationBuilder Create()
SimulationBuilder UseDefaultMigrationStages()
SimulationBuilder AddStage(ISimulationStage stage)
SimulationBuilder AddObserver(ISimulationObserver observer)
SimulationBuilder WithConsoleOutput(bool colored = true)
SimulationBuilder WithSimulationConfig(SimulationConfig config)
SimulationBuilder ConfigureSimulation(Action<SimulationConfigBuilder> configure)
SimulationBuilder WithModelConfig(StandardModelConfig config)
SimulationBuilder ConfigureModel(Action<ModelConfigBuilder> configure)
SimulationBuilder WithAttractionCalculator(IAttractionCalculator calculator)
SimulationBuilder WithMigrationCalculator(IMigrationCalculator calculator)
SimulationBuilder WithRandomSeed(int seed)
SimulationEngine Build()
```

**Example:**

```csharp
using dotMigrata.Simulation.Builders;
using dotMigrata.Simulation.Metrics;

// Create simulation engine with fluent API
var engine = SimulationBuilder.Create()
    .WithConsoleOutput(colored: true)
    .WithRandomSeed(42)
    .ConfigureSimulation(s => s
        .MaxSteps(100)
        .StabilityThreshold(50))
    .ConfigureModel(m => m
        .CapacitySteepness(3.0)
        .DistanceDecayLambda(0.002))
    .Build();

var result = await engine.RunAsync(world);
```

## Version 3.0+ Features

### Value Specifications

Version 3.0+ uses direct `ValueSpec` static methods for type-safe value creation:

**ValueSpec Methods:**

```csharp
// Use ValueSpec static methods directly (no using static needed)
ValueSpec.Fixed(0.75)                      // Fixed value
ValueSpec.InRange(0.4, 0.8)                // Uniform distribution  
ValueSpec.Approximately(0.6, 0.15)         // Normal distribution
ValueSpec.RandomWithScale(1.5)             // Random with scale

// Generic method for custom attributes
Attribute("CustomAttribute").InRange(0, 100)
```

### Configuration Validation

All configuration objects now support strict validation:

**StandardModelConfig:**

```csharp
var config = new StandardModelConfig
{
    CapacitySteepness = 5.0,              // Must be > 0
    DistanceDecayLambda = 0.001,          // Must be >= 0
    MigrationProbabilitySteepness = 10.0, // Must be > 0
    MigrationProbabilityThreshold = 0.0,
    FactorSmoothingAlpha = 0.2,           // Must be in [0, 1]
    UseParallelProcessing = true,          // Enable/disable parallelism
    MaxDegreeOfParallelism = null          // Optional parallelism limit
}.Validate();  // Throws ConfigurationException if invalid
```

### Enhanced Exception System

Version 3.0 provides a comprehensive exception hierarchy:

- `DotMigrataException` - Base exception for all framework exceptions
- `ConfigurationException` - Invalid configuration values
- `GeneratorSpecificationException` - Invalid generator specifications
- `WorldValidationException` - World structure validation errors (includes city name and missing factors)
- `SnapshotException` - Snapshot serialization/deserialization errors (includes file path)

**Example:**

```csharp
try
{
    var world = new World([city1, city2], [factor1, factor2])
    {
        DisplayName = "My World"
    };
}
catch (WorldValidationException ex)
{
    Console.WriteLine($"City '{ex.CityName}' is missing factors:");
    foreach (var factor in ex.MissingFactorNames)
        Console.WriteLine($"  - {factor}");
}
```

### Simulation Lifecycle Hooks

The `ISimulationStageLifecycle` interface is available for future extension. The SimulationEngine does not currently

```csharp
public class MyCustomStage : ISimulationStage, ISimulationStageLifecycle
{
    public string Name => "MyStage";
    
    public void OnSimulationStart(SimulationContext context)
    {
        // Custom initialization logic
    }
    
    public void OnSimulationEnd(SimulationContext context)
    {
        // Custom cleanup logic
    }
    
    public Task ExecuteAsync(SimulationContext context)
    {
        // Called each step by SimulationEngine
        return Task.CompletedTask;
    }
}
```

### Custom Stability Criteria

The `IStabilityCriteria` interface and `DefaultStabilityCriteria` implementation are available for future extension.
The SimulationEngine currently uses built-in stability checking. The interface is designed for custom stability
detection strategies:

```csharp
public class MyStabilityCriteria : IStabilityCriteria
{
    public bool ShouldCheckStability(SimulationContext context, SimulationConfig config)
    {
        // Determine when to check stability
        return context.CurrentStep % 10 == 0;
    }
    
    public bool IsStable(SimulationContext context, SimulationConfig config)
    {
        // Determine if simulation is stable
        return /* your logic */;
    }
}
```

To use custom stability criteria, you would need to extend `SimulationEngine` or create a custom
implementation.

### Snapshot Validation

Version 3.0 adds methods for snapshot validation:

```csharp
// Non-throwing deserialization
if (XmlSnapshotSerializer.TryDeserializeFromFile(path, out var snapshot, out var error))
    Console.WriteLine("Snapshot loaded successfully");
else
    Console.WriteLine($"Failed to load snapshot: {error}");

// Quick validation
if (XmlSnapshotSerializer.ValidateSnapshot(path))
    Console.WriteLine("Snapshot is valid");
```

## Simulation Metrics

### MetricsCollector

Collects and aggregates simulation metrics over time for academic analysis.

**Properties:**

- `History` (IReadOnlyList<SimulationMetrics>) - Complete metrics history
- `CurrentMetrics` (SimulationMetrics?) - Most recent metrics snapshot
- `AverageMigrationRate` (double) - Average migration rate across all steps
- `TotalMigrations` (long) - Total migrations across all steps

**Methods:**

```csharp
SimulationMetrics Collect(World world, int step, ...)
void Clear()
string ExportToCsv()
```

### SimulationMetrics

Snapshot of simulation metrics at a specific step.

**Properties:**

- `Step` (int) - Step number
- `TotalPopulation` (int) - Total population
- `MigrationCount` (int) - Migrations this step
- `MigrationRate` (double) - Migrations per person
- `CityMetrics` (IReadOnlyList<CityMetrics>) - Per-city metrics
- `TagPopulations` (IReadOnlyDictionary<string, int>) - Population by tag
- `PopulationGiniCoefficient` (double) - Population distribution inequality (0-1)
- `PopulationEntropy` (double) - Population distribution evenness
- `PopulationCoefficientOfVariation` (double) - Population std dev / mean

### MetricsObserver

Observer that automatically collects metrics at each step.

**Example:**

```csharp
using dotMigrata.Simulation.Metrics;

var metricsObserver = new MetricsObserver();
engine.AddObserver(metricsObserver);

var result = await engine.RunAsync(world);

// Access collected metrics
var metrics = metricsObserver.Collector;
Console.WriteLine($"Average migration rate: {metrics.AverageMigrationRate:P2}");
Console.WriteLine($"Total migrations: {metrics.TotalMigrations}");

// Export to CSV
File.WriteAllText("metrics.csv", metrics.ExportToCsv());
```