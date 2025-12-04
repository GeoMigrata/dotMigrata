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
- `AllPersons` (IEnumerable<Person>) - All persons across all cities
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

- `DisplayName` (string) - Display name
- `Area` (double) - Area in square kilometers
- `Location` (Coordinate) - Geographic coordinates
- `Capacity` (int?) - Maximum population capacity (optional)
- `FactorValues` (IReadOnlyList<FactorValue>) - Factor values for this city
- `Persons` (IReadOnlyCollection<Person>) - Persons residing in this city
- `Population` (int) - Total population (count of persons)

**Methods:**

- `UpdateFactorIntensity(FactorDefinition, double)` - Updates a factor's intensity
- `TryGetFactorValue(FactorDefinition, out FactorValue?)` - Gets a factor value
- `AddPerson(Person)` - Adds a person to this city
- `RemovePerson(Person)` - Removes a person from this city
- `TryGetPerson(string, out Person?)` - Gets a person by ID

### Person

Represents an individual person with unique attributes and migration preferences.

**Properties:**

- `Id` (string) - Unique identifier
- `CurrentCity` (City?) - Current city where the person resides
- `MovingWillingness` (double, 0-1) - Willingness to migrate
- `RetentionRate` (double, 0-1) - Tendency to stay in current location
- `SensitivityScaling` (double) - Attraction scaling coefficient
- `AttractionThreshold` (double) - Minimum attraction difference for migration
- `MinimumAcceptableAttraction` (double) - Minimum destination attraction
- `FactorSensitivities` (IReadOnlyDictionary<FactorDefinition, double>) - Factor sensitivities
- `Tags` (IReadOnlyList<string>) - Tags for categorization and statistical analysis

**Constructor:**

```csharp
new Person(string id, IDictionary<FactorDefinition, double> factorSensitivities)
```

**Methods:**

- `GetSensitivity(FactorDefinition)` - Gets sensitivity value for a factor
- `UpdateSensitivity(FactorDefinition, double)` - Updates sensitivity for a factor

### FactorDefinition

Defines a city characteristic that influences migration.

**Properties:**

- `DisplayName` (string) - Factor name
- `Type` (FactorType) - Positive or Negative
- `MinValue` (double) - Minimum value for normalization
- `MaxValue` (double) - Maximum value for normalization
- `Transform` (TransformType?) - Normalization transform (Linear, Log, Sigmoid)

### FactorValue

Represents a factor's intensity value for a city.

**Properties:**

- `Definition` (FactorDefinition) - The factor definition
- `Intensity` (double) - Raw intensity value

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

- `IdPrefix` (string) - ID prefix for generated persons (default: "P")
- `SpecificationCount` (int) - Number of specifications in the collection

**Methods:**

```csharp
PersonCollection Add(IndividualSpecification specification)
PersonCollection Add(IndividualsSpecification specification)
PersonCollection Add(GeneratorSpecification specification)
PersonCollection Add(IPersonSpecification specification)
IEnumerable<Person> GenerateAllPersons(IEnumerable<FactorDefinition> factorDefinitions)
int GetTotalCount()
void Clear()
```

### IPersonSpecification

Interface for person specifications (Individual, Individuals, Generator).

**Methods:**

```csharp
IEnumerable<Person> GeneratePersons(
    IEnumerable<FactorDefinition> factorDefinitions,
    Func<string> idGenerator)
```

### IndividualSpecification

Specifies a single manually-defined person.

**Properties:**

- `FactorSensitivities` (Dictionary<string, double>) - Factor sensitivities by factor name
- `MovingWillingness` (double, 0-1) - Required
- `RetentionRate` (double, 0-1) - Required
- `SensitivityScaling` (double) - Default: 1.0
- `AttractionThreshold` (double) - Default: 0.0
- `MinimumAcceptableAttraction` (double) - Default: 0.0
- `Tags` (IReadOnlyList<string>) - Tags for categorization

### IndividualsSpecification

Specifies multiple identical persons (duplicates).

**Properties:**

- `Count` (int) - Number of duplicate persons - Required
- `FactorSensitivities` (Dictionary<string, double>) - Required
- `MovingWillingness` (double, 0-1) - Required
- `RetentionRate` (double, 0-1) - Required
- `SensitivityScaling` (double) - Default: 1.0
- `AttractionThreshold` (double) - Default: 0.0
- `MinimumAcceptableAttraction` (double) - Default: 0.0
- `Tags` (IReadOnlyList<string>) - Tags for categorization

### GeneratorSpecification

Generates persons with randomized or specified attributes.

**Constructors:**

```csharp
new GeneratorSpecification() // True random
new GeneratorSpecification(int seed) // Pseudo-random with seed
```

**Properties:**

- `Count` (int) - Number of persons to generate - Required
- `FactorSensitivities` (Dictionary<string, ValueSpecification>) - Factor sensitivity specifications
- `MovingWillingness` (ValueSpecification?) - Willingness specification
- `RetentionRate` (ValueSpecification?) - Retention specification
- `SensitivityScaling` (ValueSpecification?) - Scaling specification
- `AttractionThreshold` (ValueSpecification?) - Threshold specification
- `MinimumAcceptableAttraction` (ValueSpecification?) - Min attraction specification
- `Tags` (IReadOnlyList<string>) - Tags to assign to all generated persons
- `DefaultMinSensitivity` (double) - Default: -10.0
- `DefaultMaxSensitivity` (double) - Default: 10.0
- `DefaultSensitivityStdDev` (double) - Default: 3.0

### ValueSpecification

Represents a value specification for attributes (fixed, ranged, or biased).

**Static Factory Methods:**

```csharp
static ValueSpecification Fixed(double value)
static ValueSpecification InRange(double min, double max)
static ValueSpecification Random()
static ValueSpecification RandomWithScale(double scale)
```

**Methods:**

```csharp
ValueSpecification WithScale(double scale)
```

**Properties:**

- `IsFixed` (bool) - Whether this is a fixed value
- `HasRange` (bool) - Whether this has a custom range
- `FixedValue` (double?) - The fixed value if applicable
- `Range` ((double Min, double Max)?) - The range if applicable
- `Scale` (double) - The scale factor

**Examples:**

```csharp
// Fixed value - all persons get 5.0
ValueSpecification.Fixed(5.0)

// Random in range 0.3 to 0.8
ValueSpecification.InRange(0.3, 0.8)

// Random with default range
ValueSpecification.Random()

// Random with default range, scaled by 1.5 (bias higher)
ValueSpecification.Random().WithScale(1.5)

// Random in range, scaled by 0.5 (bias lower)
ValueSpecification.InRange(0.2, 0.8).WithScale(0.5)
```

## Generator (Legacy)

### PersonGenerator

Generates persons with randomized attributes and factor sensitivities.

**Constructor:**

```csharp
new PersonGenerator(PersonGeneratorConfig? config = null)
```

**Methods:**

```csharp
IEnumerable<Person> GeneratePersons(
    int count,
    IEnumerable<FactorDefinition> factorDefinitions,
    string idPrefix = "P")

Person GeneratePerson(
    string id,
    IEnumerable<FactorDefinition> factorDefinitions)

void GenerateAndDistributePersons(
    int totalCount,
    IEnumerable<FactorDefinition> factorDefinitions,
    IDictionary<City, int> cityDistribution,
    string idPrefix = "P")
```

### PersonGeneratorConfig

Configuration for person generation.

**Properties:**

- `MinMovingWillingness` (double, 0-1) - Default: 0.1
- `MaxMovingWillingness` (double, 0-1) - Default: 0.9
- `MinRetentionRate` (double, 0-1) - Default: 0.1
- `MaxRetentionRate` (double, 0-1) - Default: 0.9
- `MinSensitivity` (double) - Default: -10.0
- `MaxSensitivity` (double) - Default: 10.0
- `SensitivityStdDev` (double) - Default: 3.0
- `MinSensitivityScaling` (double) - Default: 0.5
- `MaxSensitivityScaling` (double) - Default: 2.0
- `RandomSeed` (int?) - For reproducible generation, default: null

**Static:**

- `Default` - Default configuration instance

## Configuration

### SimulationConfig

Controls simulation execution.

**Properties:**

- `MaxTicks` (int) - Maximum simulation ticks (default: 1000)
- `CheckStability` (bool) - Whether to check for stability (default: true)
- `StabilityThreshold` (int) - Population change threshold for stability (default: 10)
- `StabilityCheckInterval` (int) - How often to check stability (default: 1)
- `MinTicksBeforeStabilityCheck` (int) - Minimum ticks before checking (default: 10)

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
    Person person, 
    City? originCity = null)

IDictionary<City, AttractionResult> CalculateAttractionForAllCities(
    IEnumerable<City> cities, 
    Person person, 
    City? originCity = null)
```

**Implementations:**

- `StandardAttractionCalculator` - Standard model implementation

### IMigrationCalculator

Interface for calculating individual migration decisions.

**Methods:**

```csharp
MigrationFlow? CalculateMigrationDecision(
    Person person,
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
void OnTickStart(SimulationContext context)
void OnStageComplete(string stageName, SimulationContext context)
void OnTickComplete(SimulationContext context)
void OnSimulationEnd(SimulationContext context, string reason)
void OnError(SimulationContext context, Exception exception)
```

**Implementations:**

- `ConsoleObserver` - Prints to console with person-based statistics

## Simulation Pipeline

### Creating a Simulation

To create and run a simulation:

```csharp
using dotMigrata.Logic.Calculators;
using dotMigrata.Simulation.Engine;
using dotMigrata.Simulation.Interfaces;
using dotMigrata.Simulation.Pipeline;
using dotMigrata.Simulation.Models;

// Create calculators
var attractionCalc = new StandardAttractionCalculator();
var migrationCalc = new StandardMigrationCalculator();

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
- `CurrentTick` (int) - Current tick number
- `IsStabilized` (bool) - Whether the simulation has stabilized
- `TotalPopulationChange` (int) - Total population change in current tick
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
- **Immutable Person Properties**: Persons regenerated from specifications on load

**XML Structure with Namespaces:**

```xml

<Snapshot xmlns="http://geomigrata.pages.dev/snapshot"
          xmlns:c="http://geomigrata.pages.dev/code"
          Version="1.0" Id="..." Status="Seed" CreatedAt="..." CurrentStep="0">
    <World DisplayName="Example World">
        <!-- FactorDefinitions is a snapshot container (default namespace) -->
        <FactorDefinitions>
            <!-- c:FactorDefinition maps to FactorDefinition class in code -->
            <c:FactorDefinition Id="pollution" DisplayName="Pollution" Type="Negative" Min="0" Max="12"
                                Transform="Linear"/>
        </FactorDefinitions>

        <!-- PersonCollections are snapshot-only constructs -->
        <PersonCollections>
            <PersonCollection Id="young_people">
                <!-- c:Person maps to Person class with Count attribute for duplication -->
                <c:Person Count="1" MovingWillingness="0.5" RetentionRate="0.3"
                          AttractionThreshold="0.3" SensitivityScaling="0.5"
                          MinimumAcceptableAttraction="10" Tags="tag1;tag2">
                    <FactorSensitivities>
                        <Sensitivity Id="pollution" Value="0.5"/>
                    </FactorSensitivities>
                </c:Person>

                <!-- Generator uses ValueSpecifications for mass person generation -->
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

**ValueSpecification Types:**

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
5. **Collections "expand" at startup** - Generators run to produce actual Person instances

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
- `Person` (Person) - The individual person migrating
- `MigrationProbability` (double, 0-1) - Migration probability

## Enums

### FactorType

- `Positive` - Increases attraction (e.g., income)
- `Negative` - Decreases attraction (e.g., pollution)

### TransformType

- `Linear` - Linear normalization
- `Log` - Logarithmic normalization
- `Sigmoid` - Sigmoid normalization

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
var outputSnapshot = SnapshotConverter.ToSnapshot(result.World, SnapshotStatus.Completed, result.CurrentTick);
XmlSnapshotSerializer.SerializeToFile(outputSnapshot, "output.xml");
```

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
        .MaxTicks(100)
        .StabilityThreshold(50))
    .ConfigureModel(m => m
        .CapacitySteepness(3.0)
        .DistanceDecayLambda(0.002))
    .Build();

var result = await engine.RunAsync(world);
```

## Simulation Metrics

### MetricsCollector

Collects and aggregates simulation metrics over time for academic analysis.

**Properties:**

- `History` (IReadOnlyList<SimulationMetrics>) - Complete metrics history
- `CurrentMetrics` (SimulationMetrics?) - Most recent metrics snapshot
- `AverageMigrationRate` (double) - Average migration rate across all ticks
- `TotalMigrations` (long) - Total migrations across all ticks

**Methods:**

```csharp
SimulationMetrics Collect(World world, int tick, ...)
void Clear()
string ExportToCsv()
```

### SimulationMetrics

Snapshot of simulation metrics at a specific tick.

**Properties:**

- `Tick` (int) - Tick number
- `TotalPopulation` (int) - Total population
- `MigrationCount` (int) - Migrations this tick
- `MigrationRate` (double) - Migrations per person
- `CityMetrics` (IReadOnlyList<CityMetrics>) - Per-city metrics
- `TagPopulations` (IReadOnlyDictionary<string, int>) - Population by tag
- `PopulationGiniCoefficient` (double) - Population distribution inequality (0-1)
- `PopulationEntropy` (double) - Population distribution evenness
- `PopulationCoefficientOfVariation` (double) - Population std dev / mean

### MetricsObserver

Observer that automatically collects metrics at each tick.

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