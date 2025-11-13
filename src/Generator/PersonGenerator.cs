using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Values;

namespace dotGeoMigrata.Generator;

/// <summary>
/// Generates persons with randomized attributes and factor sensitivities for population simulation.
/// This generator creates large numbers of individuals (10,000 to 1,000,000+) with varied characteristics.
/// </summary>
public sealed class PersonGenerator
{
    private readonly PersonGeneratorConfig _config;
    private readonly Random _random;

    /// <summary>
    /// Initializes a new instance of the PersonGenerator class.
    /// </summary>
    /// <param name="config">Configuration for person generation. If null, uses default configuration.</param>
    public PersonGenerator(PersonGeneratorConfig? config = null)
    {
        _config = config ?? PersonGeneratorConfig.Default;
        _random = _config.RandomSeed.HasValue
            ? new Random(_config.RandomSeed.Value)
            : new Random();
    }

    /// <summary>
    /// Generates a collection of persons with randomized attributes.
    /// </summary>
    /// <param name="count">Number of persons to generate.</param>
    /// <param name="factorDefinitions">Factor definitions for which to generate sensitivities.</param>
    /// <param name="idPrefix">Optional prefix for person IDs. Default is "P".</param>
    /// <returns>A collection of generated persons.</returns>
    /// <exception cref="ArgumentException">Thrown when count is less than 1.</exception>
    /// <exception cref="ArgumentNullException">Thrown when factorDefinitions is null.</exception>
    public IEnumerable<Person> GeneratePersons(
        int count,
        IEnumerable<FactorDefinition> factorDefinitions,
        string idPrefix = "P")
    {
        if (count < 1)
            throw new ArgumentException("Count must be at least 1.", nameof(count));

        ArgumentNullException.ThrowIfNull(factorDefinitions);

        var factors = factorDefinitions.ToList();
        if (factors.Count == 0)
            throw new ArgumentException("Must provide at least one factor definition.", nameof(factorDefinitions));

        var persons = new List<Person>(count);

        for (var i = 0; i < count; i++)
        {
            var personId = $"{idPrefix}{i + 1:D10}";
            var person = GeneratePerson(personId, factors);
            persons.Add(person);
        }

        return persons;
    }

    /// <summary>
    /// Generates a single person with randomized attributes.
    /// </summary>
    /// <param name="id">Unique identifier for the person.</param>
    /// <param name="factorDefinitions">Factor definitions for which to generate sensitivities.</param>
    /// <returns>A generated person with randomized attributes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when id or factorDefinitions is null.</exception>
    public Person GeneratePerson(string id, IEnumerable<FactorDefinition> factorDefinitions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(factorDefinitions);

        var factors = factorDefinitions.ToList();
        var sensitivities = new Dictionary<FactorDefinition, double>();

        // Generate sensitivities for each factor using normal distribution
        foreach (var factor in factors)
        {
            var sensitivity = GenerateNormalRandom(0, _config.SensitivityStdDev);
            sensitivity = Math.Clamp(sensitivity, _config.MinSensitivity, _config.MaxSensitivity);
            sensitivities[factor] = sensitivity;
        }

        var person = new Person(id, sensitivities)
        {
            MovingWillingness = GenerateUniformRandom(_config.MinMovingWillingness, _config.MaxMovingWillingness),
            RetentionRate = GenerateUniformRandom(_config.MinRetentionRate, _config.MaxRetentionRate),
            SensitivityScaling = GenerateUniformRandom(_config.MinSensitivityScaling, _config.MaxSensitivityScaling),
            AttractionThreshold = GenerateUniformRandom(-0.1, 0.1),
            MinimumAcceptableAttraction = GenerateUniformRandom(0.0, 0.2)
        };

        return person;
    }

    /// <summary>
    /// Generates persons and distributes them across cities according to specified distribution.
    /// </summary>
    /// <param name="totalCount">Total number of persons to generate and distribute.</param>
    /// <param name="factorDefinitions">Factor definitions for which to generate sensitivities.</param>
    /// <param name="cityDistribution">Dictionary mapping cities to their target population counts.</param>
    /// <param name="idPrefix">Optional prefix for person IDs. Default is "P".</param>
    /// <exception cref="ArgumentException">Thrown when totalCount doesn't match sum of distribution.</exception>
    public void GenerateAndDistributePersons(
        int totalCount,
        IEnumerable<FactorDefinition> factorDefinitions,
        IDictionary<City, int> cityDistribution,
        string idPrefix = "P")
    {
        ArgumentNullException.ThrowIfNull(cityDistribution);

        var distributionSum = cityDistribution.Values.Sum();
        if (distributionSum != totalCount)
            throw new ArgumentException(
                $"City distribution sum ({distributionSum}) must equal total count ({totalCount}).",
                nameof(cityDistribution));

        var allPersons = GeneratePersons(totalCount, factorDefinitions, idPrefix).ToList();
        var personIndex = 0;

        foreach (var (city, count) in cityDistribution)
        {
            for (var i = 0; i < count && personIndex < allPersons.Count; i++, personIndex++)
            {
                city.AddPerson(allPersons[personIndex]);
            }
        }
    }

    /// <summary>
    /// Generates a uniform random value between min and max.
    /// </summary>
    private double GenerateUniformRandom(double min, double max)
    {
        return min + _random.NextDouble() * (max - min);
    }

    /// <summary>
    /// Generates a normally distributed random value using Box-Muller transform.
    /// </summary>
    private double GenerateNormalRandom(double mean, double stdDev)
    {
        // Box-Muller transform
        var u1 = 1.0 - _random.NextDouble();
        var u2 = 1.0 - _random.NextDouble();
        var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return mean + stdDev * randStdNormal;
    }
}