namespace dotMigrata.Simulation.Display;

/// <summary>
/// Provides pre-configured display option presets for common use cases.
/// </summary>
public static class DisplayPresets
{
    /// <summary>
    /// Standard console output preset - shows essential simulation information.
    /// Includes world info, step summaries, top cities, and completion details.
    /// </summary>
    public static DisplayOption Console =>
        DisplayOption.WorldInfo |
        DisplayOption.StepSummary |
        DisplayOption.TopCities |
        DisplayOption.CompletionInfo |
        DisplayOption.FinalDistribution;

    /// <summary>
    /// Debug output preset - shows comprehensive simulation information.
    /// Includes all available display options for detailed troubleshooting.
    /// </summary>
    public static DisplayOption Debug =>
        DisplayOption.All;

    /// <summary>
    /// Minimal output preset - shows only critical completion information.
    /// Useful for background or automated simulations.
    /// </summary>
    public static DisplayOption Minimal =>
        DisplayOption.CompletionInfo;

    /// <summary>
    /// Verbose output preset - shows detailed information without individual person samples.
    /// Balances detail with readability for large-scale simulations.
    /// </summary>
    public static DisplayOption Verbose =>
        DisplayOption.WorldInfo |
        DisplayOption.FactorDefinitions |
        DisplayOption.CityDetails |
        DisplayOption.PopulationByTags |
        DisplayOption.StepHeader |
        DisplayOption.StageProgress |
        DisplayOption.MigrationFlows |
        DisplayOption.StepSummary |
        DisplayOption.PopulationChanges |
        DisplayOption.CompletionInfo |
        DisplayOption.FinalDistribution |
        DisplayOption.MigrationStats |
        DisplayOption.PerformanceMetrics;

    /// <summary>
    /// Silent preset - displays nothing (same as <see cref="DisplayOption.None" />).
    /// </summary>
    public static DisplayOption Silent =>
        DisplayOption.None;
}