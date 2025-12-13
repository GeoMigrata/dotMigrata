namespace dotMigrata.Core.Exceptions;

/// <summary>
/// Represents validation errors that occur when validating world structure,
/// such as missing factor values in cities or mixing person types.
/// </summary>
public sealed class WorldValidationException : DotMigrataException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorldValidationException" /> class
    /// for missing factor values.
    /// </summary>
    /// <param name="cityName">The name of the city that failed validation.</param>
    /// <param name="missingFactorNames">The names of the factors that are missing values.</param>
    public WorldValidationException(string cityName, IEnumerable<string> missingFactorNames)
        : base($"City '{cityName}' is missing values for factors: {string.Join(", ", missingFactorNames)}")
    {
        ArgumentNullException.ThrowIfNull(missingFactorNames);

        CityName = cityName;
        MissingFactorNames = missingFactorNames.ToArray();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WorldValidationException" /> class
    /// with a custom message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public WorldValidationException(string message)
        : base(message)
    {
        CityName = string.Empty;
        MissingFactorNames = [];
    }

    /// <summary>
    /// Gets the name of the city that failed validation.
    /// </summary>
    public string CityName { get; }

    /// <summary>
    /// Gets the names of the factors that are missing values.
    /// </summary>
    public IReadOnlyList<string> MissingFactorNames { get; }
}