# API Reference

This document provides a comprehensive reference for the public API of dotGeoMigrata.

## Main Entry Points

### WorldBuilder

Fluent builder for creating `World` instances.

```csharp
var world = new WorldBuilder()
    .WithName("My World")
    .AddFactor(name, type, min, max, transform)
    .AddPopulationGroup(name, willingness, retention, configureSensitivities)
    .AddCity(name, lat, lon, area, capacity, configureCity)
    .Build();
```

**Methods:**

- `WithName(string)` - Sets the world display name
- `AddFactor(string, FactorType, double, double, TransformType?)` - Adds a factor definition
- `AddFactor(FactorDefinition)` - Adds a pre-configured factor
- `AddPopulationGroup(string, double, double, Action<GroupDefinitionBuilder>)` - Adds a population group
- `AddPopulationGroup(GroupDefinition)` - Adds a pre-configured group
- `AddCity(string, double, double, double, int?, Action<CityBuilder>)` - Adds a city
- `AddCity(City)` - Adds a pre-configured city
- `Build()` - Builds and validates the world

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

Top-level entity containing cities, factor definitions, and population groups.

**Properties:**

- `DisplayName` (string) - Display name of the world
- `Cities` (IReadOnlyList&lt;City&gt;) - List of cities
- `FactorDefinitions` (IReadOnlyList&lt;FactorDefinition&gt;) - List of factor definitions
- `GroupDefinitions` (IReadOnlyList&lt;GroupDefinition&gt;) - List of population group definitions
- `Population` (int) - Total population across all cities

**Constructor:**

```csharp
new World(
    IEnumerable<City> cities,
    IEnumerable<FactorDefinition> factorDefinitions,
    IEnumerable<GroupDefinition> populationGroupDefinitions)
{ DisplayName = "..." }
```

### City

Represents a city with geographic location, factors, and population groups.

**Properties:**

- `DisplayName` (string) - Display name
- `Area` (double) - Area in square kilometers
- `Location` (Coordinate) - Geographic coordinates
- `Capacity` (int?) - Maximum population capacity (optional)
- `FactorValues` (IReadOnlyList&lt;FactorValue&gt;) - Factor values for this city
- `PopulationGroupValues` (IReadOnlyList&lt;GroupValue&gt;) - Population group values
- `Population` (int) - Total population

**Methods:**

- `UpdateFactorIntensity(FactorDefinition, double)` - Updates a factor's intensity
- `TryGetFactorValue(FactorDefinition, out FactorValue?)` - Gets a factor value
- `UpdatePopulationCount(GroupDefinition, int)` - Updates population for a group
- `TryGetPopulationGroupValue(GroupDefinition, out GroupValue?)` - Gets a group value

### FactorDefinition

Defines a city characteristic that influences migration.

**Properties:**

- `DisplayName` (string) - Factor name
- `Type` (FactorType) - Positive or Negative
- `MinValue` (double) - Minimum value for normalization
- `MaxValue` (double) - Maximum value for normalization
- `Transform` (TransformType?) - Normalization transform (Linear, Log, Sigmoid)

### GroupDefinition

Defines a population group's migration behavior.

**Properties:**

- `DisplayName` (string) - Group name
- `MovingWillingness` (double, 0-1) - Willingness to migrate
- `RetentionRate` (double, 0-1) - Tendency to stay
- `SensitivityScaling` (double) - Attraction scaling coefficient
- `AttractionThreshold` (double) - Minimum attraction difference for migration
- `MinimumAcceptableAttraction` (double) - Minimum destination attraction
- `Sensitivities` (IReadOnlyList&lt;FactorSensitivity&gt;) - Factor sensitivities

**Constructor:**

```csharp
new GroupDefinition(IEnumerable<FactorSensitivity> sensitivities)
{
    DisplayName = "...",
    MovingWillingness = 0.5,
    RetentionRate = 0.5
}
```

### FactorValue

Represents a factor's intensity value for a city.

**Properties:**

- `Definition` (FactorDefinition) - The factor definition
- `Intensity` (double) - Raw intensity value

### GroupValue

Represents population count for a group in a city.

**Properties:**

- `Definition` (GroupDefinition) - The group definition
- `Population` (int) - Population count

### Coordinate

Geographic coordinate (WGS84).

**Properties:**

- `Latitude` (double, -90 to 90) - Latitude in degrees
- `Longitude` (double, -180 to 180) - Longitude in degrees

**Methods:**

- `DistanceTo(Coordinate)` - Calculate distance in kilometers
- `static DistanceBetween(Coordinate, Coordinate)` - Calculate distance between two coordinates

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

Interface for calculating city attraction.

**Methods:**

```csharp
AttractionResult CalculateAttraction(City city, GroupDefinition group, City? originCity = null);
IDictionary<City, AttractionResult> CalculateAttractionForAllCities(
    IEnumerable<City> cities, GroupDefinition group, City? originCity = null);
```

**Implementations:**

- `StandardAttractionCalculator` - Standard model implementation

### IMigrationCalculator

Interface for calculating migration flows.

**Methods:**

```csharp
IEnumerable<MigrationFlow> CalculateMigrationFlows(
    City originCity, IEnumerable<City> destinationCities,
    GroupDefinition group, int currentPopulation,
    IDictionary<City, AttractionResult> attractionResults);
IEnumerable<MigrationFlow> CalculateAllMigrationFlows(
    World world, IAttractionCalculator attractionCalculator);
```

**Implementations:**

- `StandardMigrationCalculator` - Standard model implementation

### ISimulationStage

Represents a stage in the simulation pipeline.

**Properties:**

- `Name` (string) - Stage name

**Methods:**

```csharp
Task ExecuteAsync(SimulationContext context);
bool ShouldExecute(SimulationContext context); // Default: true
```

**Built-in Stages:**

- `AttractionCalculationStage` - Calculates attractions
- `MigrationDecisionStage` - Determines migration flows
- `MigrationExecutionStage` - Executes migrations

### ISimulationObserver

Observer pattern for monitoring simulation.

**Methods:**

```csharp
void OnSimulationStart(SimulationContext context);
void OnTickStart(SimulationContext context);
void OnStageComplete(string stageName, SimulationContext context);
void OnTickComplete(SimulationContext context);
void OnSimulationEnd(SimulationContext context, string reason);
void OnError(SimulationContext context, Exception exception);
```

**Implementations:**

- `ConsoleObserver` - Prints to console

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
- `CurrentMigrationFlows` (IEnumerable&lt;MigrationFlow&gt;) - Current migration flows

**Methods:**

- `SetData<T>(string key, T value)` - Stores data for inter-stage communication
- `T? GetData<T>(string key)` - Retrieves stored data
- `bool TryGetData<T>(string key, out T? value)` - Tries to retrieve data

## Snapshot System

### SnapshotService

Static service for creating and restoring snapshots.

**Methods:**

```csharp
static WorldSnapshot CreateSnapshot(
    World world,
    SnapshotStatus status = SnapshotStatus.Seed,
    IEnumerable<SimulationStep>? steps = null);

static World RestoreWorld(WorldSnapshot snapshot);

static MigrationRecord CreateMigrationRecord(MigrationFlow flow);
```

### JsonSnapshotSerializer

JSON serialization for snapshots.

**Methods:**

```csharp
static string Serialize(WorldSnapshot snapshot, JsonSerializerOptions? options = null);
static void SerializeToFile(WorldSnapshot snapshot, string filePath, JsonSerializerOptions? options = null);
static WorldSnapshot? Deserialize(string json, JsonSerializerOptions? options = null);
static WorldSnapshot? DeserializeFromFile(string filePath, JsonSerializerOptions? options = null);
```

### XmlSnapshotSerializer

XML serialization for snapshots (similar interface to JSON).

## Result Models

### AttractionResult

Result of attraction calculation.

**Properties:**

- `City` (City) - The evaluated city
- `BaseAttraction` (double, 0-1) - Base attraction from factors
- `AdjustedAttraction` (double, 0-1) - Adjusted for capacity and distance
- `CapacityResistance` (double, 0-1) - Capacity resistance factor
- `DistanceResistance` (double, 0-1) - Distance decay factor

### MigrationFlow

Represents a migration from origin to destination.

**Properties:**

- `OriginCity` (City) - Origin city
- `DestinationCity` (City) - Destination city
- `Group` (GroupDefinition) - Population group
- `MigrationCount` (double) - Number of migrants (can be fractional)
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