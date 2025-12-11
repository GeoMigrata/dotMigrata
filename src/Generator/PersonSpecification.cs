using dotMigrata.Core.Entities;
using dotMigrata.Core.Values;

namespace dotMigrata.Generator;

/// <summary>
/// Specifies a person or group of persons to be added to a population.
/// Can represent a single person instance, duplicates of a person, or a generator for random persons.
/// </summary>
public sealed class PersonSpecification
{
    private readonly GeneratorConfig? _generator;
    private readonly PersonBase? _template;

    private PersonSpecification(PersonBase? template, GeneratorConfig? generator, int count)
    {
        _template = template;
        _generator = generator;
        Count = count;
    }

    /// <summary>
    /// Gets whether this specification uses a person template.
    /// </summary>
    public bool IsTemplate => _template != null;

    /// <summary>
    /// Gets whether this specification uses a generator.
    /// </summary>
    public bool IsGenerator => _generator != null;

    /// <summary>
    /// Gets the count of persons this specification will generate.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Creates a specification from a single person instance.
    /// </summary>
    /// <param name="person">The person to add.</param>
    /// <returns>A person specification.</returns>
    public static PersonSpecification FromPerson(PersonBase person)
    {
        ArgumentNullException.ThrowIfNull(person);
        return new PersonSpecification(person, null, 1);
    }

    /// <summary>
    /// Creates a specification from duplicates of a person.
    /// </summary>
    /// <param name="person">The person template to duplicate.</param>
    /// <param name="count">Number of duplicates to create.</param>
    /// <returns>A person specification.</returns>
    public static PersonSpecification FromPerson(PersonBase person, int count)
    {
        ArgumentNullException.ThrowIfNull(person);
        return count <= 0
            ? throw new ArgumentException("Count must be positive.", nameof(count))
            : new PersonSpecification(person, null, count);
    }

    /// <summary>
    /// Creates a specification from a generator configuration.
    /// </summary>
    /// <param name="generator">The generator configuration.</param>
    /// <returns>A person specification.</returns>
    public static PersonSpecification FromGenerator(GeneratorConfig generator)
    {
        ArgumentNullException.ThrowIfNull(generator);
        return new PersonSpecification(null, generator, generator.Count);
    }

    /// <summary>
    /// Generates persons according to this specification.
    /// </summary>
    /// <param name="factorDefinitions">Factor definitions for the world.</param>
    /// <returns>Generated persons.</returns>
    internal IEnumerable<PersonBase> GeneratePersons(IEnumerable<FactorDefinition> factorDefinitions)
    {
        if (_template != null)
        {
            // Generate duplicates from template
            // For StandardPerson, preserve all properties
            if (_template is StandardPerson stdTemplate)
            {
                for (var i = 0; i < Count; i++)
                {
                    var sensitivities = stdTemplate.FactorSensitivities.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value);

                    var person = new StandardPerson(sensitivities)
                    {
                        MovingWillingness = stdTemplate.MovingWillingness,
                        RetentionRate = stdTemplate.RetentionRate,
                        SensitivityScaling = stdTemplate.SensitivityScaling,
                        AttractionThreshold = stdTemplate.AttractionThreshold,
                        MinimumAcceptableAttraction = stdTemplate.MinimumAcceptableAttraction,
                        Tags = stdTemplate.Tags.ToList()
                    };

                    yield return person;
                }
            }
            else
            {
                // For custom person types, create basic copies
                // Custom types should handle their own cloning logic if needed
                for (var i = 0; i < Count; i++)
                    yield return _template;
            }
        }
        else if (_generator != null)
        {
            // Generate from generator configuration
            foreach (var person in _generator.GeneratePersons(factorDefinitions))
                yield return person;
        }
    }
}