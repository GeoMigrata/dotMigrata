namespace dotMigrata.Core.Exceptions;

/// <summary>
/// Represents validation errors that occur when validating world structure,
/// such as missing factor values in cities.
/// </summary>
public sealed class WorldValidationException : DotMigrataException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WorldValidationException"/> class.
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
    /// Gets the name of the city that failed validation.
    /// </summary>
    public string CityName { get; }

    /// <summary>
    /// Gets the names of the factors that are missing values.
    /// </summary>
    public IReadOnlyList<string> MissingFactorNames { get; }
}