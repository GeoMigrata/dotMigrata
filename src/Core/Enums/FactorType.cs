namespace dotMigrata.Core.Enums;

/// <summary>
/// Defines the direction of a factor's influence on city attractiveness.
/// </summary>
public enum FactorType
{
    /// <summary>
    /// Negative factors reduce attractiveness (e.g., pollution, crime).
    /// </summary>
    Negative,

    /// <summary>
    /// Positive factors increase attractiveness (e.g., income, services).
    /// </summary>
    Positive
}