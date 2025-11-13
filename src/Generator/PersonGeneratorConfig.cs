namespace dotGeoMigrata.Generator;

/// <summary>
/// Configuration for person generation with randomized attributes.
/// </summary>
public sealed record PersonGeneratorConfig
{
    /// <summary>
    /// Gets the default configuration for person generation.
    /// </summary>
    public static PersonGeneratorConfig Default { get; } = new();

    /// <summary>
    /// Gets or initializes the minimum moving willingness value (0-1).
    /// Default is 0.1.
    /// </summary>
    public double MinMovingWillingness { get; init; } = 0.1;

    /// <summary>
    /// Gets or initializes the maximum moving willingness value (0-1).
    /// Default is 0.9.
    /// </summary>
    public double MaxMovingWillingness { get; init; } = 0.9;

    /// <summary>
    /// Gets or initializes the minimum retention rate value (0-1).
    /// Default is 0.1.
    /// </summary>
    public double MinRetentionRate { get; init; } = 0.1;

    /// <summary>
    /// Gets or initializes the maximum retention rate value (0-1).
    /// Default is 0.9.
    /// </summary>
    public double MaxRetentionRate { get; init; } = 0.9;

    /// <summary>
    /// Gets or initializes the minimum factor sensitivity value.
    /// Default is -10.0.
    /// </summary>
    public double MinSensitivity { get; init; } = -10.0;

    /// <summary>
    /// Gets or initializes the maximum factor sensitivity value.
    /// Default is 10.0.
    /// </summary>
    public double MaxSensitivity { get; init; } = 10.0;

    /// <summary>
    /// Gets or initializes the standard deviation for sensitivity distribution.
    /// Higher values create more varied sensitivities. Default is 3.0.
    /// </summary>
    public double SensitivityStdDev { get; init; } = 3.0;

    /// <summary>
    /// Gets or initializes the minimum sensitivity scaling coefficient.
    /// Default is 0.5.
    /// </summary>
    public double MinSensitivityScaling { get; init; } = 0.5;

    /// <summary>
    /// Gets or initializes the maximum sensitivity scaling coefficient.
    /// Default is 2.0.
    /// </summary>
    public double MaxSensitivityScaling { get; init; } = 2.0;

    /// <summary>
    /// Gets or initializes the random seed for reproducible generation.
    /// If null, uses a time-based seed. Default is null.
    /// </summary>
    public int? RandomSeed { get; init; } = null;
}