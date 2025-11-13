# API Reference

This document provides a comprehensive reference for the public API of dotGeoMigrata with the new Person-based
architecture.

## Main Entry Points

### WorldBuilder

Fluent builder for creating `World` instances with person-based populations.

```csharp
var world = new WorldBuilder()
    .WithName("My World")
    .AddFactor(name, type, min, max, transform)
    .AddCity(name, lat, lon, area, capacity, configureCity)
    .Build();
```

**Methods:**

- `WithName(string)` - Sets the world display name
- `AddFactor(string, FactorType, double, double, TransformType?)` - Adds a factor definition
- `AddFactor(FactorDefinition)` - Adds a pre-configured factor
- `AddCity(string, double, double, double, int?, Action<CityBuilder>)` - Adds a city
- `AddCity(City)` - Adds a pre-configured city
- `WithRandomPopulation(int, IDictionary<string, int>?, PersonGeneratorConfig?)` - Populates world with random persons
- `Build()` - Builds and validates the world

### CityBuilder

Helper builder for configuring city properties and initial population.

```csharp
city => city
    .WithFactorValue("Income", 50000)
    .WithRandomPersons(100000)
    .WithRandomPersons(50000, personConfig)
```

**Methods:**

- `WithFactorValue(string, double)` - Sets the intensity value for a specific factor
- `WithPerson(Person)` - Adds a single person to the city
- `WithPersons(IEnumerable<Person>)` - Adds multiple persons to the city
- `WithRandomPersons(int, PersonGeneratorConfig?, string?)` - Generates and adds random persons

### SimulationBuilder

Fluent builder for creating and configuring `SimulationEngine` instances.

```csharp
var engine = new SimulationBuilder()
    .WithWorld(world)
    .WithModelConfig(modelConfig)
    .WithSimulationConfig(simConfig)
    .UseStandardPipeline()
    .AddConsoleObserver()
    .Build();
```

**Methods:**

- `WithWorld(World)` - Sets the world to simulate (required)
- `WithSimulationConfig(SimulationConfig?)` - Configures simulation parameters
- `WithModelConfig(StandardModelConfig?)` - Configures model parameters
- `WithAttractionCalculator(IAttractionCalculator)` - Sets custom attraction calculator
- `WithMigrationCalculator(IMigrationCalculator)` - Sets custom migration calculator
- `AddCustomStage(ISimulationStage)` - Adds a custom pipeline stage
- `UseStandardPipeline()` - Uses the default simulation pipeline
- `AddObserver(ISimulationObserver)` - Adds a simulation observer
- `AddConsoleObserver(bool)` - Adds a console observer (colored output optional)
- `Build()` - Builds the simulation engine
- `BuildAndRunAsync()` - Builds and immediately runs the simulation

## Core Models

### World

Top-level entity containing cities and factor definitions.

**Properties:**

- `DisplayName` (string) - Display name of the world
- `Cities` (IReadOnlyList<City>) - List of cities
- `FactorDefinitions` (IReadOnlyList<FactorDefinition>) - List of factor definitions
- `AllPersons` (IEnumerable<Person>) - All persons across all cities
- `Population` (int) - Total population across all cities

**Constructor:**

```csharp
new World(
    IEnumerable<City> cities,
    IEnumerable<FactorDefinition> factorDefinitions)
{ DisplayName = "..." }
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

### SnapshotService

Static service for creating snapshots.

**Methods:**

```csharp
static WorldSnapshot CreateSnapshot(
    World world,
    SnapshotStatus status = SnapshotStatus.Seed,
    IEnumerable<SimulationStep>? steps = null)

static World RestoreWorld(WorldSnapshot snapshot) // Currently throws NotImplementedException

static MigrationRecord CreateMigrationRecord(MigrationFlow flow)
```

*Note: Full person-based snapshot restoration is pending implementation.*

### JsonSnapshotSerializer

JSON serialization for snapshots.

**Methods:**

```csharp
static string Serialize(WorldSnapshot snapshot, JsonSerializerOptions? options = null)
static void SerializeToFile(WorldSnapshot snapshot, string filePath, JsonSerializerOptions? options = null)
static WorldSnapshot? Deserialize(string json, JsonSerializerOptions? options = null)
static WorldSnapshot? DeserializeFromFile(string filePath, JsonSerializerOptions? options = null)
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
- `InProgress` - Simulation in progress
- `Completed` - Simulation completed

## Migration from Old API

If you're migrating from the PopulationGroup-based API, see `MIGRATION_GUIDE.md` for detailed migration instructions.

**Key Changes:**

- **Removed**: `GroupDefinition`, `GroupValue`, `AddPopulationGroup()` methods
- **New**: `Person` entity, `PersonGenerator`, `WithRandomPersons()` method
- **Modified**: `World` constructor, calculator interfaces, `MigrationFlow` model