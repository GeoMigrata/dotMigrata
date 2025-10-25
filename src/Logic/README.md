# Logic Layer Documentation

This directory contains the core logic for calculating attraction, migration flows, and feedback effects in the simulation.

## Overview

The Logic layer consists of three main components:

1. **Attraction** - Calculates city attractiveness for population groups
2. **Migration** - Determines migration flows based on attraction differences
3. **Feedback** - Updates city factors after migration

---

## Attraction Calculator

### Algorithm Overview

The `AttractionCalculator` computes how attractive a city is to a specific population group based on the city's factor values and the group's sensitivities.

### Calculation Steps

1. **For each factor sensitivity of the population group:**
    - Retrieve the city's factor value for that factor
    - Normalize the factor value using the factor's normalization rules (linear, log, or sigmoid)
    - Determine the factor type (Positive or Negative)
        - Use `OverriddenFactorType` from sensitivity if specified
        - Otherwise use the factor definition's type
    - Calculate contribution:
        - **Positive factors**: `contribution = normalizedValue × sensitivity`
        - **Negative factors**: `contribution = -normalizedValue × sensitivity`

2. **Sum all contributions** to get the total attraction score

### Mathematical Formula

```
AttractionScore = Σ(contribution_i) for all factors i

where:
  contribution_i = {
    normalizedValue_i × sensitivity_i     if factor is Positive
    -normalizedValue_i × sensitivity_i    if factor is Negative
  }
  
  normalizedValue_i = normalize(rawValue_i, factor_i)
```

### Normalization Types

- **Linear**: `(value - min) / (max - min)`
- **Logarithmic**: `log(value - min + δ) / log(max - min + δ)` where δ = 1e-6
- **Sigmoid**: `1 / (1 + exp(-steepness × (linear - 0.5)))` where steepness = 10.0

### Example

For a population group "Young Professionals" with sensitivities:
- Income (Positive, sensitivity: 10)
- Pollution (Negative, sensitivity: 8)
- Public Services (Positive, sensitivity: 6)

For a city with normalized values:
- Income: 0.7
- Pollution: 0.4
- Public Services: 0.8

Attraction Score = (0.7 × 10) + (-0.4 × 8) + (0.8 × 6) = 7.0 - 3.2 + 4.8 = **8.6**

---

## Migration Calculator

### Algorithm Overview

The `MigrationCalculator` determines actual migration flows between cities based on attraction differences, distance costs, and probabilistic sampling.

### Calculation Steps

1. **For each destination city** (excluding the source city):

   a. **Calculate attraction difference**:
   ```
   attractionDiff = destinationAttraction - sourceAttraction
   ```

   b. **Check minimum threshold**:
    - Skip if `attractionDiff ≤ minimumAttractionThreshold` (default: 0.1)

   c. **Calculate migration cost**:
   ```
   distance = haversineDistance(sourceCity, destinationCity)
   migrationCost = baseMigrationCost × distance
   ```
    - `baseMigrationCost` default: 0.001

   d. **Calculate net attraction**:
   ```
   netAttraction = attractionDiff - migrationCost
   ```
    - Skip if `netAttraction ≤ 0`

   e. **Calculate migration probability**:
   ```
   baseProbability = movingWillingness × (1 - retentionRate)
   attractionFactor = tanh(netAttraction)
   migrationProbability = baseProbability × attractionFactor
   ```

   f. **Sample actual migrants**:
    - For small populations (≤100): Binomial sampling (each person independently)
    - For large populations (>100): Normal approximation to binomial
      ```
      mean = totalPopulation × probability
      variance = totalPopulation × probability × (1 - probability)
      stdDev = sqrt(variance)
      sample = mean + stdDev × N(0,1)  [using Box-Muller transform]
      actualMigrants = clamp(round(sample), 0, totalPopulation)
      ```

2. **Return all migration flows** where `actualMigrants > 0`

### Key Parameters

- **BaseMigrationCost**: 0.001 (cost per km of distance)
- **MinimumAttractionThreshold**: 0.1 (minimum attraction difference to consider)
- **MovingWillingness**: Group-specific (0-1)
- **RetentionRate**: Group-specific (0-1)

### Example

For a population group with:
- Count: 10,000
- MovingWillingness: 0.3
- RetentionRate: 0.7

From City A (attraction: 5.0) to City B (attraction: 8.0):
- Distance: 500 km
- AttractionDiff: 3.0
- MigrationCost: 0.001 × 500 = 0.5
- NetAttraction: 3.0 - 0.5 = 2.5
- BaseProbability: 0.3 × (1 - 0.7) = 0.09
- AttractionFactor: tanh(2.5) ≈ 0.987
- MigrationProbability: 0.09 × 0.987 ≈ 0.089
- Expected Migrants: 10,000 × 0.089 = 890
- Actual Migrants: ~890 (sampled from normal distribution)

---

## Feedback Calculator

### Algorithm Overview

The `FeedbackCalculator` updates city factors after migration based on population changes. This creates a feedback loop where migration affects city conditions, which in turn affects future attraction.

### Design Principles

1. **Population-Dependent Effects**: Factors change based on population ratios
2. **Smoothing**: Gradual changes to avoid abrupt fluctuations
3. **Categorized Impacts**: Different types of factors respond differently

### Feedback Categories

#### 1. Per-Capita Resources
Factors like public services, healthcare, and education that are distributed among population:
- **Effect**: Inversely proportional to population
- **Example**: If population doubles, per-capita service quality decreases

#### 2. Housing and Congestion
Factors like housing costs, traffic congestion, and pollution:
- **Effect**: Increases with population density
- **Example**: More people → higher housing demand → higher costs

#### 3. Economic Factors
Factors like economic output, innovation, and job opportunities:
- **Effect**: Complex relationship with population
- **Positive**: Larger workforce, more innovation
- **Negative**: Potential infrastructure strain

### Smoothing Formula

To prevent sudden jumps in factor values:

```
newValue = currentValue + smoothingFactor × (targetValue - currentValue)
```

Where:
- `smoothingFactor` ∈ [0, 1] (default: 0.3)
- Higher smoothingFactor = more gradual changes

### Implementation Note

The current `FeedbackCalculator` provides the framework and smoothing mechanism. The actual factor updates depend on the specific factors defined in each simulation. Users can extend this class or implement custom feedback logic based on their specific factor definitions.

### Example Feedback Rules (Illustrative)

For a city experiencing 20% population growth:

**Per-Capita Resources:**
```
newPublicServices = smooth(current, current × 0.83)  // ~17% decrease
```

**Housing/Congestion:**
```
newHousingCost = smooth(current, current × 1.15)  // ~15% increase
newPollution = smooth(current, current × 1.10)    // ~10% increase
```

**Economic:**
```
newEconomicOutput = smooth(current, current × 1.18)  // ~18% increase
```

These specific ratios would be configured based on the simulation's requirements and the nature of each factor.

---

## Integration Flow

The three components work together in each simulation step:

```
1. AttractionCalculator.CalculateAttractionForAllCities()
   → Produces attraction scores for each city-group pair

2. MigrationCalculator.CalculateMigrationFlows()
   → Produces migration flows based on attractions

3. Apply migrations (update population counts)

4. FeedbackCalculator.ApplyFeedback()
   → Update city factors based on new populations

5. Repeat for next step
```

This creates a dynamic feedback system where city characteristics drive migration, and migration reshapes city characteristics over time.