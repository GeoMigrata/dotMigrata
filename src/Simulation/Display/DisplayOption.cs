namespace dotMigrata.Simulation.Display;

/// <summary>
/// Specifies what information to display during simulation execution.
/// Multiple options can be combined using bitwise OR operations.
/// </summary>
[Flags]
public enum DisplayOption : long
{
    /// <summary>
    /// Display nothing (silent mode).
    /// </summary>
    None = 0,

    #region Simulation Start Options

    /// <summary>
    /// Display basic world information (name, city count, total population).
    /// </summary>
    WorldInfo = 1L << 0,

    /// <summary>
    /// Display the list of factor definitions with their types.
    /// </summary>
    FactorDefinitions = 1L << 1,

    /// <summary>
    /// Display detailed city information (location, population, capacity, factors).
    /// </summary>
    CityDetails = 1L << 2,

    /// <summary>
    /// Display initial population distribution grouped by tags.
    /// </summary>
    PopulationByTags = 1L << 3,

    #endregion

    #region Step Progress Options

    /// <summary>
    /// Display step number header/banner at the start of each step.
    /// </summary>
    StepHeader = 1L << 4,

    /// <summary>
    /// Display stage completion notifications during step execution.
    /// </summary>
    StageProgress = 1L << 5,

    /// <summary>
    /// Display migration flow details (routes and counts).
    /// </summary>
    MigrationFlows = 1L << 6,

    /// <summary>
    /// Display individual person migration samples with details.
    /// </summary>
    PersonSamples = 1L << 7,

    #endregion

    #region Step Complete Options

    /// <summary>
    /// Display step summary (migration counts).
    /// </summary>
    StepSummary = 1L << 8,

    /// <summary>
    /// Display population changes per city with deltas from initial.
    /// </summary>
    PopulationChanges = 1L << 9,

    /// <summary>
    /// Display top N cities by population.
    /// </summary>
    TopCities = 1L << 10,

    #endregion

    #region Simulation End Options

    /// <summary>
    /// Display completion information (total steps, reason, final population).
    /// </summary>
    CompletionInfo = 1L << 11,

    /// <summary>
    /// Display final population distribution across all cities.
    /// </summary>
    FinalDistribution = 1L << 12,

    /// <summary>
    /// Display total migration statistics and analysis.
    /// </summary>
    MigrationStats = 1L << 13,

    /// <summary>
    /// Display performance metrics (timing, memory, throughput).
    /// </summary>
    PerformanceMetrics = 1L << 14,

    #endregion

    #region Composite Options

    /// <summary>
    /// Display all available information (equivalent to debug mode).
    /// </summary>
    All = WorldInfo | FactorDefinitions | CityDetails | PopulationByTags |
          StepHeader | StageProgress | MigrationFlows | PersonSamples |
          StepSummary | PopulationChanges | TopCities |
          CompletionInfo | FinalDistribution | MigrationStats | PerformanceMetrics

    #endregion
}