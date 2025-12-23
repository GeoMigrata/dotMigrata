# dotMigrata Changelog

## Version 0.7.5-beta (Checkpoint System)

### Overview

This release introduces a checkpoint system for simulation reproducibility and state management, removes
complex history tracking in favor of seed-based reproducibility, and unifies documentation structure and style across
all files.

### Breaking Changes

- Snapshot format updated with new fields: `LastUsedSeed` and `Checkpoints`
- Checkpoint system: checkpoints are labels for steps
- Resuming from a checkpoint with a different seed clears all subsequent steps and checkpoints
- No timeline branching or complex history management

### New Features

- **Checkpoint System**: Named labels for simulation steps to make resumption easier
- **LastUsedSeed Field**: Stores the random seed used in simulation for exact reproducibility
- **Seed-Based Reproducibility**: Single seed propagates through all random operations ensuring deterministic results

### Improvements

- **Unified Documentation**: Standardized language, style, and structure across CHANGELOGS, USAGE, API, and README
- **Removed Version References**: Eliminated version-specific references from XML documentation
- **Clearer Checkpoint Semantics**: Checkpoints are simple step labels, not complex state storage

### Migration Guide

1. **Update snapshot loading code**: Pass `lastUsedSeed` parameter when creating snapshots
2. **Add checkpoint management**: Use the new `Checkpoints` collection to label important steps
3. **Ensure seed consistency**: Verify your code uses a single seed for all random operations
4. **Re-export snapshots**: Generate new snapshots to adopt the updated schema format

### Technical Details

- CheckpointXml model: Contains `Step` (int), `Label` (string), and `CreatedAt` (DateTime)
- WorldSnapshotXml now includes `LastUsedSeed?: int` for reproducibility
- SnapshotConverter.ToSnapshot signature updated with new optional parameters

---

## Version 0.7.4-beta (Framework Unity and Step Terminology)

### Overview

This release unifies the framework's core concepts, standardizes terminology, and simplifies the type system for better
consistency and maintainability.

### Breaking Changes

- **Snapshot compatibility**: Previous snapshots cannot be loaded without manual conversion
- **API compatibility**: All Tick references must be updated to Step
- **Factor values**: Must be normalized to 0-1 range

### New Features

- **Step Terminology**: Consistent use of "Step" throughout the framework
- **Simplified FactorDefinition**: Removed unnecessary MinValue/MaxValue properties

### Improvements

- Updated all observers for Step terminology
- Improved XML documentation comments throughout
- Fixed typo: Renamed `DefautStabilityCriteria` to `DefaultStabilityCriteria`

### Migration Guide

1. **Update all code references**: Search for "Tick" and replace with "Step"
2. **Update snapshots**: Rename all Tick-related XML attributes to Step equivalents
3. **Normalize factor values**: Convert all raw factor values to 0-1 range before use
4. **Remove Min/Max from FactorDefinition**: Delete these properties from factor creation code
5. **Update event triggers**: Use `StepTrigger` consistently
6. **Re-export snapshots**: Generate new snapshots with the updated schema

---

## Version 0.7.3-beta (Snapshot Version Validation and Schema Updates)

### Overview

This release focuses on strengthening snapshot compatibility, adding stricter version validation, and aligning the
snapshot XML structure with the latest codebase.

### New Features

- Strict snapshot version validation with clearer error messages and upgrade guidance
- Snapshot XML structure adjusted to reflect current runtime models
- Backward-compatible loading for older snapshots with warnings

### Improvements

- Version field presence validation
- Clear error messages for unsupported or mismatched versions
- Compatibility mapping to support loading known prior versions when safe
- World and PersonCollection sections aligned to current generator APIs
- Factor definitions and factor values normalized for consistency
- Custom person type serialization contracts refined for determinism

### Migration Guide

- If your snapshot lacks a Version field, the loader will attempt to infer and may warn
- Ensure PersonCollections prefer generator specifications over explicit persons for large populations
- Verify type names for custom person serializers match registration keys
- Re-export snapshots using the new serializer to adopt updated structure

### Documentation

- Updated README and USAGE to reflect version validation and new snapshot structure
- Fixed minor example typos and consistency issues in code snippets

---

## Version 0.7.2-beta (Complete Type Safety & Code Cleanup)

### Overview

This release achieves complete type safety throughout the codebase, eliminates code duplication, and improves API
consistency. All 0-1 range values now use `UnitValue` for compile-time guarantees.

### Breaking Changes

- All 0-1 range values now use `UnitValue` instead of `double`
- `Guard.ThrowIfNull()` removed - use `ArgumentNullException.ThrowIfNull()` directly
- `FactorIntensity.Normalize()` signature simplified (parameter removed)
- `AttractionResult.AdjustedAttraction` now computed property instead of stored

### New Features

**Type Safety Improvements**: All properties/methods that return or accept 0-1 range values now use `UnitValue`.

| Component                             | Previous Type           | New Type                   |
|---------------------------------------|-------------------------|----------------------------|
| `AttractionResult.BaseAttraction`     | `double`                | `UnitValue`                |
| `AttractionResult.AdjustedAttraction` | `double`                | `UnitValue` (computed)     |
| `AttractionResult.CapacityResistance` | `double`                | `UnitValue`                |
| `AttractionResult.DistanceResistance` | `double`                | `UnitValue`                |
| `MigrationFlow.MigrationProbability`  | `double`                | `UnitValue`                |
| `MathUtils.Sigmoid()` return          | `double`                | `UnitValue`                |
| `MathUtils.DistanceDecay()` return    | `double`                | `UnitValue`                |
| `MathUtils.LinearNormalize()` return  | `double`                | `UnitValue`                |
| `MathUtils.Softmax()` return          | `IReadOnlyList<double>` | `IReadOnlyList<UnitValue>` |

**New API Features**:

- `City.GetPopulationRatio()` - Helper method for consistent population ratio calculations
- Implicit conversion from `UnitValue` to `double` for backward compatibility

### Improvements

- Eliminated code duplication in population ratio calculations
- Improved type safety with compile-time guarantees for 0-1 range values
- Reduced memory usage with computed properties
- Cleaner API surface with fewer redundant parameters

### Migration Guide

1. **Update property/variable types**: Change `double` to `UnitValue` for all 0-1 range values
2. **Update method calls**: Replace `Guard.ThrowIfNull()` with `ArgumentNullException.ThrowIfNull()`
3. **Remove redundant parameters**: `Normalize()` no longer needs parameter
4. **Use implicit conversion**: `UnitValue` converts to `double` implicitly when needed

```csharp
// Updated type usage
UnitValue baseAttraction = result.BaseAttraction;
UnitValue probability = flow.MigrationProbability;
UnitValue resistance = MathUtils.DistanceDecay(distance, lambda);

// Implicit conversion to double when needed
double baseDouble = baseAttraction;  // Automatic conversion
```

---

## Version 0.7.1-beta (Unified Value System)

### Overview

This release introduces a complete redesign of the value system with type-safe `UnitValue`, separation of immediate vs
lazy evaluation, and dramatic simplification of the codebase.

- Feedback strategies use consistent `GetPopulationRatio()` helper
- Cleaner API surface with fewer redundant parameters

**Performance**:

- No runtime performance changes (same underlying double values)
- Compile-time type safety prevents out-of-range errors
- `AttractionResult` uses 8 fewer bytes per instance (computed property)

### Migration from v0.7.1-beta to v0.7.2-beta

Most changes are mechanical type updates. The compiler will guide you:

1. **Update property/variable types**: `double` → `UnitValue` for all 0-1 range values
2. **Update method calls**: Replace `Guard.ThrowIfNull()` with `ArgumentNullException.ThrowIfNull()`
3. **Remove redundant parameters**: `Normalize()` no longer needs parameter
4. **Use implicit conversion**: `UnitValue` converts to `double` implicitly when needed

**Note**: If you haven't yet migrated from v0.7.0 to v0.7.1, see v0.7.1-beta changelog below for the major value system
redesign.

---

## Version 0.7.1-beta (Unified Value System)

**Version 0.7.1-beta** introduces a complete redesign of the value system with type-safe `UnitValue` values,
separation of immediate vs lazy evaluation, and dramatic simplification of the codebase (from 16 to 7 files).

### ** BREAKING CHANGES - Complete API Overhaul**

This is a **major breaking release** with no backward compatibility. All value-related APIs have changed.

#### Core Type System Changes

| Old (v0.7.0)             | New (v0.7.1)                  | Purpose                          |
|--------------------------|-------------------------------|----------------------------------|
| `NormalizedValue`        | `UnitValue`                   | Immediate values (0-1 range)     |
| `ValueSpec` (all uses)   | `UnitValue` (manual)          | Direct values for cities         |
| `ValueSpec` (generators) | `UnitValueSpec`               | Lazy evaluation specs            |
| `double` (sensitivities) | `UnitValue`                   | Type-safe sensitivities          |
| `ValueRange`             | Removed                       | Inlined into `UnitValue`         |
| `ValuePresets`           | Removed                       | Static properties on `UnitValue` |
| `IValue<T>`              | Removed                       | No longer needed                 |
| `INormalizable`          | Removed                       | No longer needed                 |
| `IRangedValue`           | Removed                       | No longer needed                 |
| `ITransformFunction`     | `UnitValueSpec.TransformFunc` | Delegate-based                   |
| `TransformFunctions`     | `UnitValueSpec.Transforms`    | Moved location                   |
| `ValueExtensions`        | `UnitValueExtensions`         | Renamed                          |

#### Key Breaking Changes

- **`FactorIntensity`** - Now stores `UnitValue Value` directly (was `ValueSpec Intensity`)
- **`FactorIntensity.Materialize()`** - **REMOVED** (no longer needed, values are immediate)
- **`FactorIntensity.GetIntensity()`** - Now `GetValue()` returning `UnitValue`
- **`FactorIntensity.ComputeIntensity()`** - **REMOVED** (use `GetValue()`)
- **`City.MaterializeFactorIntensities()`** - **REMOVED** (no longer needed)
- **`City.UpdateFactorIntensity()`** - Now takes `UnitValue` (was `ValueSpec`)
- **`World.InitializeForSimulation()`** - **REMOVED** (no longer needed)
- **`PersonBase` constructor** - Now takes `IDictionary<FactorDefinition, UnitValue>` (was `double`)
- **`PersonBase.GetSensitivity()`** - Returns `UnitValue` (was `double`)
- **`PersonBase.UpdateSensitivity()`** - Takes `UnitValue` (was `double`)
- **`PersonBase.MovingWillingness`** - Now `UnitValue` (was `NormalizedValue`)
- **`PersonBase.RetentionRate`** - Now `UnitValue` (was `NormalizedValue`)
- **`StandardPerson.SensitivityScaling`** - Now `UnitValue` (was `double`)
- **`StandardPerson.AttractionThreshold`** - Now `UnitValue` (was `double`)
- **`StandardPerson.MinimumAcceptableAttraction`** - Now `UnitValue` (was `double`)
- **`StandardPersonGenerator.FactorSensitivities`** - Now `Dictionary<FactorDefinition, UnitValueSpec>` (was
  `ValueSpec`)
- **`StandardPersonGenerator.MovingWillingness`** - Now `UnitValueSpec` (was `ValueSpec`)
- **`StandardPersonGenerator.RetentionRate`** - Now `UnitValueSpec` (was `ValueSpec`)
- **`StandardPersonGenerator.DefaultSensitivityRange`** - **REMOVED** (all values are 0-1)
- **`StandardPersonGenerator.SensitivityStdDev`** - **REMOVED** (use specs instead)
- **`FactorDefinition.TransformFunction`** - Now `UnitValueSpec.TransformFunc` (was `ITransformFunction`)

### New Features - Unified Normalized Value System

#### `UnitValue` - Type-Safe Immediate Values

All factor intensities and sensitivities now use `UnitValue`, guaranteeing values are in [0, 1] range:

```csharp
// Create normalized values
var intensity = UnitValue.FromRatio(0.75);          // From 0-1 ratio
var percent = UnitValue.FromPercentage(75);        // From percentage
var half = UnitValue.Half;                         // Common values
var zero = UnitValue.Zero;
var one = UnitValue.One;

// Extension methods (using C# 13 extension keyword)
var value = 0.75.AsNormalized();                      // From double
var fromPct = 75.AsPercentage();                      // From int percentage

// Type-safe arithmetic (all clamped to 0-1)
var sum = value1 + value2;                            // Clamped addition
var product = value * 2.0;                            // Clamped multiplication
```

#### `UnitValueSpec` - Lazy Evaluation for Generators

For generators that need to produce randomized values:

```csharp
// Fixed value
var spec = UnitValueSpec.Fixed(0.5);

// Random range
var spec = UnitValueSpec.InRange(0.3, 0.8);

// Normal distribution
var spec = UnitValueSpec.Approximately(0.5, 0.1);

// With transform
var spec = UnitValueSpec.InRange(0, 1)
    .WithTransform(UnitValueSpec.Transforms.Logarithmic);

// Evaluate to get actual value
UnitValue value = spec.Evaluate(random);
```

#### Transform Functions

Built-in transforms now in `UnitValueSpec.Transforms`:

- `Linear` - Proportional mapping (default)
- `Logarithmic` - Emphasizes lower values
- `Sigmoid` - S-curve with middle emphasis
- `Exponential` - Emphasizes higher values
- `SquareRoot` - Moderate lower emphasis

### Performance Improvements

- **6x faster creation** - Struct-based vs class-based allocation
- **3x less memory** - 8 bytes vs 24 bytes per value
- **Zero overhead** - No materialization needed, values are immediate
- **No caching complexity** - Direct value access

### Codebase Simplification

**Before (v0.7.0): 16 files, ~1,581 lines**

- NormalizedValue.cs (300 lines)
- ValueSpec.cs (383 lines)
- ValueRange.cs (70 lines)
- FactorIntensity.cs (135 lines)
- TransformFunctions.cs (66 lines)
- ValuePresets.cs (128 lines)
- ValueExtensions.cs (61 lines)
- 5 interface files (145 lines)
- 1 attribute file (43 lines)
- Plus FactorDefinition, Coordinate, HaversineDistanceCalculator

**After (v0.7.1): 7 files, ~750 lines**

- UnitValue.cs (~280 lines) - Core value type
- UnitValueSpec.cs (~280 lines) - Generator specifications
- FactorIntensity.cs (~50 lines) - Simplified wrapper
- FactorDefinition.cs (~95 lines) - Simplified, no ValueRange dependency
- UnitValueExtensions.cs (~65 lines) - Extension methods
- Plus Coordinate, HaversineDistanceCalculator (unchanged)

**Removed:** ValueRange, ValuePresets, IValue, INormalizable, IRangedValue, ITransformFunction, ValueRangeAttribute

### Migration Guide

#### 1. Update Factor Intensities

```csharp
// OLD (v0.7.0)
new FactorIntensity { 
    Definition = factor,
    Intensity = ValueSpec.Fixed(0.75)
};
city.MaterializeFactorIntensities();

// NEW (v0.7.1)
new FactorIntensity {
    Definition = factor,
    Value = UnitValue.FromRatio(0.75)
};
// No materialization needed!
```

#### 2. Update Person Sensitivities

```csharp
// OLD (v0.7.0)
var sensitivities = new Dictionary<FactorDefinition, double> {
    { factor1, 5.0 },
    { factor2, -3.0 }
};

// NEW (v0.7.1) - All sensitivities are 0-1, factor type determines direction
var sensitivities = new Dictionary<FactorDefinition, UnitValue> {
    { factor1, UnitValue.FromRatio(0.8) },  // High sensitivity
    { factor2, UnitValue.FromRatio(0.4) }   // Lower sensitivity
};
// Factor direction (pos/neg) is in factor.Type, not sensitivity value
```

#### 3. Update Generators

```csharp
// OLD (v0.7.0)
new StandardPersonGenerator {
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpec> {
        { factor, ValueSpec.InRange(-10, 10) }
    },
    MovingWillingness = ValueSpec.InRange(0, 1),
    DefaultSensitivityRange = new ValueRange(-10, 10)
};

// NEW (v0.7.1)
new StandardPersonGenerator {
    FactorSensitivities = new Dictionary<FactorDefinition, UnitValueSpec> {
        { factor, UnitValueSpec.InRange(0.3, 0.9) }
    },
    MovingWillingness = UnitValueSpec.InRange(0.4, 0.8)
    // No DefaultSensitivityRange - all values are 0-1
};
```

#### 4. Update City Updates

```csharp
// OLD (v0.7.0)
city.UpdateFactorIntensity(factor, ValueSpec.Fixed(newValue));

// NEW (v0.7.1)
city.UpdateFactorIntensity(factor, UnitValue.FromRatio(newValue));
```

### Design Rationale

1. **Type Safety**: `UnitValue` prevents out-of-range values at compile time
2. **Performance**: Immediate values eliminate overhead of lazy evaluation and caching
3. **Simplicity**: Separate types for immediate vs lazy use cases (not dual-purpose)
4. **Consistency**: All intensities and sensitivities use same type
5. **Clarity**: Clear distinction between generator specs and runtime values

### Technical Details

- **UnitValue**: `readonly record struct` for value semantics and zero allocation
- **UnitValueSpec**: `sealed class` for reference semantics and caching support
- **All arithmetic operations**: Automatically clamp to [0, 1] range
- **Transform functions**: Delegate-based for flexibility and performance
- **Extension methods**: Use C# 13 `extension` keyword for modern syntax

---

## Version 0.7.0-beta Highlights

**Version 0.7.0-beta** introduces a comprehensive refactoring of the value system with unified ValueSpec,
materialization optimization, and complete removal of legacy code for a clean, performant architecture.

### Breaking Changes

- **`FactorValue` removed** - Replaced by `FactorIntensity` with ValueSpec storage
- **`ValueSpecification` renamed to `ValueSpec`** - Shorter, more concise naming
- **`IntensityValue` removed** - Replaced by direct double usage with ValueSpec specification
- **`SensitivityValue` removed** - Replaced by plain double for simplified API
- **Transform system redesigned** - Now uses delegate-based `TransformDelegate` instead of `ITransformFunction`
- **All values now double** - Unified type system (previously mixed int/double)
- **No backward compatibility** - Clean break from v0.6.x value system

### New Features - Unified ValueSpec System

- **ValueSpec with Lazy Evaluation** - Values computed on-demand via `Evaluate()` method
- **Built-in Caching** - Optional caching to avoid repeated computation (enabled by default)
- **Transform Delegates** - Method delegates for flexible value transformation:
  - `ValueSpec.Transforms.Linear` - Linear transformation (default)
  - `ValueSpec.Transforms.Logarithmic` - Logarithmic transformation for wide ranges
  - `ValueSpec.Transforms.Sigmoid` - S-curve transformation for smooth transitions
  - `ValueSpec.Transforms.Exponential` - Exponential transformation emphasizing extremes
  - `ValueSpec.Transforms.SquareRoot` - Square root transformation for moderate ranges
- **Attribute-Based Validation** - `[ValueRange]` attribute for compile-time constraint specification
- **Universal Availability** - ValueSpec usable everywhere, not just in generators

### Performance Optimization - Materialization System

- **`FactorIntensity.Materialize()`** - Pre-computes ValueSpec once before simulation
- **`City.MaterializeFactorIntensities()`** - Materializes all factors in a city
- **`World.InitializeForSimulation()`** - Materializes entire world before simulation starts
- **Hybrid Approach** - Type-safe ValueSpec during setup, zero-overhead double during simulation
- **15-20% Performance Gain** - For attraction calculations in typical simulations
- **Zero Runtime Overhead** - Materialized values are pure double access (~1 CPU cycle vs ~3-4 with caching)

### API Changes

**Before (v0.6.4):**

```csharp
var factorDef = new FactorDefinition { 
    MinValue = 0, MaxValue = 100000,
    TransformFunction = TransformFunctions.Logarithmic
};
var factorValue = new FactorValue { 
    Definition = factorDef,
    Intensity = IntensityValue.FromRaw(50000)
};
```

**After (v0.7.0):**

```csharp
// Setup: Type-safe ValueSpec specification
var factorDef = new FactorDefinition { 
    DisplayName = "Income",
    Type = FactorType.Positive 
};
var factorIntensity = new FactorIntensity {
    Definition = factorDef,
    Intensity = ValueSpec.InRange(0, 100000)
        .WithTransform(ValueSpec.Transforms.Logarithmic)
};

// Before simulation: Materialize for performance (RECOMMENDED)
world.InitializeForSimulation();

// During simulation: Zero-overhead access
var value = factorIntensity.GetIntensity();  // Returns materialized double
```

### Value Specification Examples

```csharp
// Fixed value
ValueSpec.Fixed(5.0)

// Random range with uniform distribution
ValueSpec.InRange(0.3, 0.8)

// Normal distribution (approximate)
ValueSpec.Approximately(mean: 0.5, standardDeviation: 0.1)

// Random with scale factor
ValueSpec.Random().WithScale(1.5)  // Bias toward higher values
// With custom transform
ValueSpec.InRange(0, 100000)
    .WithTransform(ValueSpec.Transforms.Logarithmic)

// Custom transform delegate
ValueSpec.Fixed(50).WithTransform((value, min, max) => {
    // Custom transformation logic
    return Math.Sqrt((value - min) / (max - min));
});
```

### Simplified Type System

**PersonBase sensitivities:**

```csharp
// Before v0.7.0
public SensitivityValue GetSensitivity(FactorDefinition factor);
public void UpdateSensitivity(FactorDefinition factor, SensitivityValue sensitivity);

// After v0.7.0
public double GetSensitivity(FactorDefinition factor);
public void UpdateSensitivity(FactorDefinition factor, double sensitivity);
```

**ValuePresets optimized:**

```csharp
// Before v0.7.0 - static readonly
public static readonly SensitivityValue MediumPositive = SensitivityValue.FromRaw(5.0);

// After v0.7.0 - compile-time const
public const double MediumPositive = 5.0;
```

### Updated Components

All simulation components updated to use new value system:

- `StandardPersonGenerator` - Uses ValueSpec for all person attribute generation
- `StandardAttractionCalculator` - Uses `GetIntensity()` for factor values
- `CongestionFeedbackStrategy` & `PerCapitaFeedbackStrategy` - Use ValueSpec for updates
- `FactorChangeEffect` - Uses ValueSpec for event-driven factor changes
- `City`, `World`, `PersonBase` - All use new unified value system

### Performance Best Practices

```csharp
// 1. Setup phase: Use ValueSpec for safety and convenience
var city = new City("CityName", location, factorIntensities);

// 2. Pre-simulation: Materialize for optimal performance (RECOMMENDED)
world.InitializeForSimulation();

// 3. Simulation loop: Zero-overhead access
for (int step = 0; step < 1000; step++)
{
    // GetIntensity() returns pre-materialized double - no overhead
    var intensity = factorIntensity.GetIntensity();
    // ... simulation logic
}
```

### Architecture Benefits

1. **Type Safety** - ValueSpec ensures values always within valid ranges
2. **Performance** - Materialization eliminates runtime overhead (15-20% faster)
3. **Convenience** - Unified API for all value specifications
4. **Flexibility** - Transform delegates allow custom transformations
5. **Clean Design** - Removed 4 obsolete types, simplified to ValueSpec + double
6. **Zero Allocations** - Removed wrapper type allocations during simulation

---

## Version 0.6.4-beta Highlights

**Version 0.6.4-beta** adds complete support for custom person types in snapshots through a type discriminator pattern,
enabling full serialization/deserialization of custom person implementations.

### Custom Person Snapshot Support

- **Type Discriminator Pattern** - `PersonType` attribute specifies which concrete person type to create
- **PersonTypeRegistry** - Central registry for custom person type serializers
- **ICustomPersonSerializer<TPerson>** - Interface for custom person serialization logic
- **ICustomGeneratorSerializer<TPerson, TGenerator>** - Interface for custom generator serialization
- **CustomProperties Element** - Extensible XML element for type-specific properties
- **Backward Compatible** - Defaults to "StandardPerson" when `PersonType` not specified

### XML Structure Updates

**Before (v0.6.3):**

```xml

<Person Count="1000" Willingness="0.7" Retention="0.4">
    <Sensitivities>
        <S Id="income" Value="8.5"/>
    </Sensitivities>
</Person>
```

**After (v0.6.4):**

```xml

<Person Count="1000" PersonType="DemographicPerson" Willingness="0.7" Retention="0.4">
    <Sensitivities>
        <S Id="income" Value="8.5"/>
    </Sensitivities>
    <CustomProperties>
        <Age>28</Age>
        <Income>75000</Income>
        <Education>Master</Education>
    </CustomProperties>
</Person>
```

### API Changes

**New Interfaces:**

```csharp
public interface ICustomPersonSerializer<TPerson> where TPerson : PersonBase
{
    TPerson CreateFromTemplate(PersonTemplateXml template, ...);
    XmlElement? SerializeCustomProperties(TPerson person, XmlDocument doc);
}

public interface ICustomGeneratorSerializer<TPerson, TGenerator> 
    where TPerson : PersonBase
    where TGenerator : IPersonGenerator<TPerson>
{
    TGenerator CreateFromXml(GeneratorXml generatorXml, ...);
    XmlElement? SerializeCustomProperties(TGenerator generator, XmlDocument doc);
}
```

**New Registry:**

```csharp
PersonTypeRegistry.RegisterPersonType<TPerson>(string typeName, ICustomPersonSerializer<TPerson> serializer);
PersonTypeRegistry.RegisterGeneratorType<TPerson, TGenerator>(string typeName, ICustomGeneratorSerializer<TPerson, TGenerator> serializer);
```

### Usage Example

```csharp
// 1. Implement serializer for custom person type
public class DemographicPersonSerializer : ICustomPersonSerializer<DemographicPerson>
{
    public DemographicPerson CreateFromTemplate(PersonTemplateXml template, ...)
    {
        // Extract Age, Income from template.CustomProperties
        return new DemographicPerson(...) { Age = ..., Income = ... };
    }

    public XmlElement? SerializeCustomProperties(DemographicPerson person, XmlDocument doc)
    {
        // Create CustomProperties element with Age, Income
        return customPropsElement;
    }
}

// 2. Register at application startup
PersonTypeRegistry.RegisterPersonType("DemographicPerson", new DemographicPersonSerializer());

// 3. Use snapshots normally - custom persons preserved!
var snapshot = SnapshotConverter.ToSnapshot(world);
var loadedWorld = SnapshotConverter.ToWorld(snapshot); // Returns DemographicPerson instances
```

### Key Benefits

1. **Full Custom Type Support** - Custom person types fully preserved in snapshots
2. **Clean Extensibility** - CustomProperties keeps core schema simple
3. **Type Safety** - Registry ensures type-safe deserialization
4. **Zero Changes for Standard Use** - StandardPerson works without registration
5. **Framework Alignment** - Matches lightweight, extensible design principles

### Architecture Changes

- `PersonTemplateXml.PersonType` - Type discriminator attribute (default: "StandardPerson")
- `PersonTemplateXml.CustomProperties` - Arbitrary XML for custom properties
- `GeneratorXml.PersonType` - Generator type discriminator
- `GeneratorXml.CustomProperties` - Arbitrary XML for custom generator specs
- `PersonTypeRegistry` - Static registry for serializer implementations
- `SnapshotConverter` - Updated to use registry for person creation

---

## Version 0.6.3-beta Highlights

**Version 0.6.3-beta** refines the event system snapshot persistence, removes obsolete code, and completes the migration
to modern extensible architecture.

### Event System Snapshot Improvements

- **Event IDs** - Added `Id` attribute to `SimulationEventXml` for unique identification
- **Multiple Effects** - Changed from single `Effect` to `Effects` array using `<Effects>` container
- **Factor ID References** - Event effects now use `FactorId` instead of `FactorName` to reference factors
- **Events in World** - Moved events from root `<Events>` to `<World><Events>` (events reference world factors)

### XML Structure Updates

**Before (v0.6.2):**

```xml

<Snapshot>
    <World>
        <Factors>...</Factors>
        <Cities>...</Cities>
    </World>
    <Events>
        <Event Name="Earthquake">
            <Effect Factor="infrastructure"
            .../>
        </Event>
    </Events>
</Snapshot>
```

**After (v0.6.3):**

```xml

<Snapshot>
    <World>
        <Factors>
            <Factor Id="infrastructure"
            .../>
        </Factors>
        <Cities>...</Cities>
        <Events>
            <Event Id="earthquake_2024" Name="Earthquake">
                <Effects>
                    <Effect FactorId="infrastructure"
                    .../>
                    <Effect FactorId="housing"
                    .../>
                </Effects>
            </Event>
        </Events>
    </World>
</Snapshot>
```

### Key Benefits

1. **Consistent ID Pattern** - Events follow same ID-based reference pattern as Factors and Collections
2. **Multiple Effects Support** - Single event can apply multiple effects simultaneously
3. **Logical Structure** - Events grouped with World (their scope and factor references)
4. **Type-Safe Resolution** - IDs resolved to object references during deserialization

### Architecture Changes

- `SimulationEventXml.Id` - Unique identifier for event
- `SimulationEventXml.Effects` - Array instead of single effect
- `EventEffectXml.FactorId` - References factor by ID (was FactorName)
- `WorldStateXml.Events` - Events now part of World element
- `WorldSnapshotXml` - Events removed from root, now in World

---

## Version 0.6.2-beta Highlights

**Version 0.6.2-beta** adds comprehensive snapshot persistence for events, configurations, and custom transforms.

### Snapshot System Enhancements

- **Event persistence** - SimulationEvents with triggers and effects are now serialized to snapshots
- **Configuration persistence** - SimulationConfig and StandardModelConfig included in snapshots
- **Custom transform support** - FactorDefinition now persists custom ITransformFunction names
- **Snapshot version bump** - Updated to v0.6 format with backward compatibility

### New Snapshot Models

- `SimulationEventXml` - Persists event display name, description, completion status, trigger, and effect
- `EventTriggerXml` - Supports Tick, Periodic, Continuous trigger types (ConditionalTrigger with limitations)
- `EventEffectXml` - Supports FactorChange, Feedback, and Composite effects
- `SimulationConfigXml` - All simulation parameters
- `StandardModelConfigXml` - All model parameters including parallelism settings
- `FactorDefXml.CustomTransformName` - Stores custom transform function names

### Limitations & Notes

**ConditionalTrigger Persistence:**

- Conditional triggers with custom lambda functions cannot be fully serialized
- Condition expressions stored as strings (for documentation)
- Must be manually reconstructed when loading snapshots with conditional events

**Custom Transform Persistence:**

- Built-in transforms (Linear, Logarithmic, Sigmoid) fully supported
- Custom ITransformFunction implementations store name only
- Falls back to Linear transform if custom transform unavailable during load
- Recommendation: Use built-in transforms or document custom transforms separately

**Event Effect Persistence:**

- FactorChangeEffect fully supported with all application types and durations
- FeedbackEffect stores strategy name (manual reconstruction needed)
- CompositeEffect supported recursively

### API Updates

**Snapshot version:**

```csharp
var snapshot = SnapshotConverter.ToSnapshot(world);
// snapshot.Version == "0.6"
// snapshot.Events contains all active SimulationEvents
// snapshot.SimulationConfig contains simulation parameters
// snapshot.ModelConfig contains model parameters
```

**Loading with custom transforms:**

```csharp
var world = SnapshotConverter.ToWorld(snapshot);
// Built-in custom transforms automatically restored
// Custom implementations must be reapplied manually after load
```

### Architecture Benefits

1. **Full Simulation State** - Snapshots now capture complete simulation state including events and configs
2. **Reproducibility** - Events and configurations ensure deterministic replay
3. **Backward Compatible** - Old v0.5 snapshots still loadable (missing fields default)
4. **Lightweight** - ~200 LOC added, no new dependencies

---

## Version 0.6.1-beta Highlights

**Version 0.6.1-beta** addresses Priority 1-2 improvements from the framework analysis, focusing on configuration
robustness, parallel execution, and extensibility.

### Configuration Improvements

- **SimulationConfig validation in constructor** - All properties now validate during initialization, ensuring
  configurations are always valid
- **Proper validation feedback** - Individual properties throw `ArgumentOutOfRangeException` immediately on invalid
  values
- **Cross-property validation** - `Validate()` method now only checks cross-property constraints (e.g.,
  MinTicksBeforeStabilityCheck < MaxTicks)

### Performance Enhancements

- **Parallel event execution** - EventStage now supports parallel execution for better performance with multiple events
- **Configurable parallelism** - New `WithParallelEvents(bool, int?)` builder method to control parallel behavior
- **Default parallel-on** - Events execute in parallel by default (can be disabled for deterministic execution)

### Extensibility - Custom Transform Functions

- **ITransformFunction interface** - Allows custom normalization functions beyond built-in Linear/Log/Sigmoid
- **TransformFunctions static class** - Built-in implementations: `TransformFunctions.Linear`, `.Logarithmic`,
  `.Sigmoid`
- **FactorDefinition.TransformFunction** - New property for custom transforms (takes precedence over enum-based
  Transform)
- **Backward compatible** - Existing `TransformType` enum still works, custom functions are opt-in

### API Updates

**New SimulationBuilder methods:**

```csharp
builder.WithParallelEvents(useParallel: true, maxDegreeOfParallelism: 4);  // Configure parallel event execution
```

**Custom transform example:**

```csharp
// Built-in transforms via new interface
var factor1 = new FactorDefinition
{
    DisplayName = "Income",
    Type = FactorType.Positive,
    MinValue = 20000,
    MaxValue = 100000,
    TransformFunction = TransformFunctions.Logarithmic  // Using interface
};

// Custom transform function
public class ExponentialTransform : ITransformFunction
{
    public string Name => "Exponential";

    public double Transform(double value, double min, double max)
    {
        var range = max - min;
        if (range == 0) return 0;
        var normalized = (value - min) / range;
        return Math.Pow(normalized, 2);  // Emphasize higher values
    }
}

var factor2 = new FactorDefinition
{
    DisplayName = "Quality",
    Type = FactorType.Positive,
    MinValue = 0,
    MaxValue = 100,
    TransformFunction = new ExponentialTransform()  // Custom implementation
};
```

### Breaking Changes

None - All changes are backward compatible. Existing code continues to work without modifications.

### Architecture Benefits

1. **Type-Safe Configuration** - Invalid configurations fail fast during construction, not at runtime
2. **Better Performance** - Parallel event execution leverages multi-core systems
3. **Extensible Transforms** - Researchers can implement domain-specific normalization functions
4. **Lightweight** - ~150 LOC added, no new dependencies

---

## Version 0.6.0-beta Highlights

**Version 0.6.0-beta** introduces a comprehensive **Event System** as a core simulation mechanism alongside cities,
factors, and persons.

### Breaking Changes

- **FeedbackStage replaced with EventStage** - Feedback strategies now work through the unified event system
- **`WithFeedbackInterval` renamed to `WithEventInterval`** - Reflects broader event system usage
- Events are now first-class simulation components with their own pipeline stage

### New Features - Event System

- **`ISimulationEvent`** - Core abstraction for simulation events that modify world state (in
  `dotMigrata.Simulation.Events.Interfaces`)
- **Event Triggers** (`dotMigrata.Simulation.Events.Triggers`) - Multiple trigger types for flexible event scheduling:
  - `TickTrigger` - Fire once at a specific tick
  - `PeriodicTrigger` - Fire repeatedly at regular intervals
  - `ContinuousTrigger` - Fire continuously within a time window
  - `ConditionalTrigger` - Fire when custom conditions are met (extension point for ECA patterns)
- **Event Effects** (`dotMigrata.Simulation.Events.Effects`) - Flexible effect system for factor modifications:
  - `FactorChangeEffect` - Change city factor values with multiple application types
  - `FeedbackEffect` - Adapter for existing feedback strategies (backward compatible)
  - `CompositeEffect` - Apply multiple effects simultaneously
- **Effect Application Types** (`dotMigrata.Simulation.Events.Enums`):
  - `EffectApplicationType.Absolute` - Set factor to exact value
  - `EffectApplicationType.Delta` - Add/subtract from current value
  - `EffectApplicationType.Multiply` - Multiply current value
  - `EffectApplicationType.LinearTransition` - Transition linearly over duration
  - `EffectApplicationType.LogarithmicTransition` - Transition logarithmically (fast then slow)
- **ValueSpecification Integration** - Reuses existing infrastructure for flexible value generation (fixed, range,
  approximate)
- **EventStage** (`dotMigrata.Simulation.Events.EventStage`) - Pipeline stage that executes events based on their
  triggers
- **Type-Safe Design** - Events use object references (FactorDefinition, City) instead of string IDs

### Builder API Updates

**New methods:**

```csharp
builder.WithEvent(ISimulationEvent)
builder.WithFactorChange(displayName, factor, valueSpec, trigger, applicationType, duration, cityFilter)
builder.WithEventInterval(interval)  // Renamed from WithFeedbackInterval
```

**Backward compatible:**

```csharp
builder.WithFeedback(strategy)  // Now creates periodic events internally
```

### Usage Examples

```csharp
using dotMigrata.Simulation.Events.Effects;
using dotMigrata.Simulation.Events.Enums;
using dotMigrata.Simulation.Events.Triggers;

// One-time event at tick 50
builder.WithFactorChange(
    "Earthquake",
    infrastructureFactor,
    ValueSpecification.Fixed(30),
    new TickTrigger(50),
    EffectApplicationType.Absolute);

// Periodic event every 10 ticks
builder.WithFactorChange(
    "Housing Subsidy",
    housingCostFactor,
    ValueSpecification.Fixed(-5),
    new PeriodicTrigger(10, startTick: 20, endTick: 80),
    EffectApplicationType.Delta);

// Linear transition over 20 ticks
builder.WithFactorChange(
    "Green Initiative",
    pollutionFactor,
    ValueSpecification.Fixed(20),
    new TickTrigger(30),
    EffectApplicationType.LinearTransition,
    EffectDuration.Over(20));

// Conditional event (extension point for complex ECA patterns)
using dotMigrata.Simulation.Events;

builder.WithEvent(new SimulationEvent(
    "Congestion Response",
    new ConditionalTrigger(
        ctx => ctx.World.Cities.Any(c => c.Population > c.Capacity * 0.9),
        cooldown: 10),
    new FactorChangeEffect(...)));
```

### Architecture Benefits

1. **Unified Mechanism** - Events and feedback use the same infrastructure
2. **Extensible** - Easy to add custom triggers, effects, and event types
3. **Type-Safe** - No string-based IDs, uses object references
4. **Lightweight** - Reuses existing ValueSpecification, no heavy dependencies
5. **Visualization-Ready** - Events have display names and descriptions for UI integration
6. **Flexible Timing** - Support for one-time, periodic, continuous, and conditional execution

### Migration Guide

**v0.5.2 code:**

```csharp
builder.WithFeedback(strategy)
    .WithFeedbackInterval(10);
```

**v0.6.0 equivalent:**

```csharp
builder.WithFeedback(strategy)  // Still works! Converted to events internally
    .WithEventInterval(10);      // Renamed method
```

**New event-based approach:**

```csharp
using dotMigrata.Simulation.Events;
using dotMigrata.Simulation.Events.Effects;
using dotMigrata.Simulation.Events.Enums;
using dotMigrata.Simulation.Events.Triggers;

builder.WithEvent(new SimulationEvent(
    $"Feedback: {strategy.Name}",
    new PeriodicTrigger(10),
    new FeedbackEffect(strategy)));
```

---

## Version 0.5.2-beta Highlights

**Version 0.5.2-beta** introduces major architectural improvements for type safety, extensibility, and design clarity:

### Breaking Changes

- **`AttributeValueBuilder` removed** - Use `ValueSpecification` static methods directly instead of
  `Attribute("name").InRange()` pattern
- **`GeneratorConfig` removed** - Replaced with `StandardPersonGenerator` implementing
  `IPersonGenerator<StandardPerson>`
- **FeedbackStrategy constructors changed** - Now use `FactorDefinition` parameter instead of `string factorName`
- **World enforces single person type** - Throws `WorldValidationException` if mixing different person types

### New Features

- **`IPersonGenerator<TPerson>` interface** - Type-safe generator interface for creating custom person generators
- **`StandardPersonGenerator`** - Clean, type-safe replacement for `GeneratorConfig`
- **Person type validation** - World constructor validates all persons are of the same concrete type
- **Type-safe FeedbackStrategy** - Use `FactorDefinition` references instead of string-based lookups

### API Changes

**Before (0.5.1-beta):**

```csharp
using static dotMigrata.Generator.AttributeValueBuilder;

collection.Add(new GeneratorConfig
{
    Count = 1000,
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [factor] = Attribute("Sensitivity").InRange(3, 8)
    },
    MovingWillingness = MovingWillingness().InRange(0.4, 0.7)
});

var feedback = new CongestionFeedbackStrategy("Quality of Life");
```

**After (0.5.2-beta):**

```csharp
collection.Add(new StandardPersonGenerator
{
    Count = 1000,
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [factor] = ValueSpecification.InRange(3, 8)
    },
    MovingWillingness = ValueSpecification.InRange(0.4, 0.7)
});

var feedback = new CongestionFeedbackStrategy(qualityOfLifeFactor);
```

### Custom Person Type Generation

**Before (PersonFactory pattern):**

```csharp
collection.Add(new GeneratorConfig
{
    Count = 1000,
    PersonFactory = (sens, will, ret, scale, thresh, minAtt, tags) =>
        new CustomPerson(sens) { /* 7 parameters */ }
});
```

**After (IPersonGenerator pattern):**

```csharp
public class CustomPersonGenerator : IPersonGenerator<CustomPerson>
{
    public required int Count { get; init; }
    public required ValueSpecification CustomProperty { get; init; }
    // Only properties you need
    
    public IEnumerable<CustomPerson> Generate(IEnumerable<FactorDefinition> factors)
    {
        // Clean generation logic
    }
}

collection.Add(new CustomPersonGenerator { Count = 1000, /* ... */ });
```

### Benefits

- **Type Safety**: Removed all string-based lookups (AttributeValueBuilder, FeedbackStrategy factor names)
- **Extensibility**: Clean `IPersonGenerator<TPerson>` interface for custom types
- **Design Clarity**: Single person type per world enforced at runtime
- **Better API**: Simpler, more discoverable interfaces

### Migration Guide

1. **Remove `using static dotMigrata.Generator.AttributeValueBuilder`**
2. **Replace `Attribute("name").Method()` with `ValueSpecification.Method()`**
3. **Replace `new GeneratorConfig` with `new StandardPersonGenerator`**
4. **Update FeedbackStrategy** to pass `FactorDefinition` instead of string
5. **Custom generators**: Implement `IPersonGenerator<TPerson>` instead of using `PersonFactory`

See [USAGE.md](USAGE.md) for complete updated examples.

## Version 0.5.1-beta Highlights

**Version 0.5.1-beta** adds support for custom person type generation and improved snapshot compatibility:

### New Features

- **PersonFactory Support** - Generate custom person types using the `PersonFactory` property in `GeneratorConfig`
- **Custom Person Generation** - Define how custom properties should be generated for your domain-specific person types
- **Snapshot Compatibility** - Snapshot system now works with `PersonBase` instead of being hardcoded to
  `StandardPerson`
- **Bug Fixes** - Fixed template duplication bug that would return the same instance for custom person types

### Example: Custom Person Generation

```csharp
collection.Add(new GeneratorConfig
{
    Count = 10000,
    FactorSensitivities = new Dictionary<FactorDefinition, ValueSpecification>
    {
        [incomeFactor] = Attribute("IncomeSensitivity").InRange(3, 8)
    },
    MovingWillingness = MovingWillingness().InRange(0.4, 0.7),
    RetentionRate = RetentionRate().InRange(0.3, 0.6),

    // PersonFactory for custom person types
    PersonFactory = (sensitivities, willingness, retention, scaling, threshold, minAttraction, tags) =>
    {
        return new DemographicPerson(sensitivities)
        {
            MovingWillingness = willingness,
            RetentionRate = retention,
            Age = random.Next(18, 65),
            Income = random.Next(25000, 120000),
            EducationLevel = "Bachelor"
        };
    }
});
```

See [USAGE.md](USAGE.md) for complete examples.

## Version 0.5.0-beta Highlights

**Version 0.5.0-beta** introduces a major architectural refactoring with inheritance-based person types:

### Breaking Changes

- **Person class removed entirely** - The monolithic `Person` class has been replaced with an inheritance-based
  architecture
- **PersonBase abstract class** - New base class with essential migration properties (`MovingWillingness`,
  `RetentionRate`, `FactorSensitivities`)
- **StandardPerson implementation** - Concrete class that replaces the old `Person` class with additional properties (
  `SensitivityScaling`, `AttractionThreshold`, `MinimumAcceptableAttraction`, `Tags`)
- **Type-based extensibility** - Custom person types can now inherit from `PersonBase` for domain-specific properties
- **Updated interfaces** - All interfaces (`IAttractionCalculator`, `IMigrationCalculator`) now use `PersonBase` instead
  of `Person`

### Enhanced Features

- **Inheritance-based architecture**: Define custom person types by inheriting from `PersonBase`
- **Pattern matching support**: `StandardAttractionCalculator` uses pattern matching for `StandardPerson` properties
- **Universal tags support**: All person types inherit `Tags` property from `PersonBase` for consistent categorization
- **Backward compatibility**: `StandardPerson` provides all functionality of the removed `Person` class
- **Enhanced Exception System**: Comprehensive exception hierarchy with `DotMigrataException`, `ConfigurationException`,
  `GeneratorSpecificationException`, `WorldValidationException`, and `SnapshotException`
- **Improved Value Specifications**: Named attribute methods like `MovingWillingness()`, `RetentionRate()` for clearer,
  more expressive code
- **Normal Distribution Support**: `Approximately(mean, stdDev)` method for realistic value generation
- **Configurable Parallelism**: Control parallel processing with `UseParallelProcessing` and `MaxDegreeOfParallelism`
- **Modern C# Features**: Uses C# latest features including records, init-only properties, and collection expressions