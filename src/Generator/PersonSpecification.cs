using dotMigrata.Core.Entities;
using dotMigrata.Core.Values;

namespace dotMigrata.Generator;

/// <summary>
/// Specifies a person or group of persons to be added to a population.
/// Can represent a single person instance, duplicates of a person, or a generator for random persons.
/// </summary>
public sealed class PersonSpecification
{
    private readonly IPersonGenerator<PersonBase>? _generator;
    private readonly PersonBase? _template;

    private PersonSpecification(PersonBase? template, IPersonGenerator<PersonBase>? generator, int count)
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
    /// Creates a specification from a person generator.
    /// </summary>
    /// <param name="generator">The person generator.</param>
    /// <returns>A person specification.</returns>
    /// <typeparam name="TPerson">The type of person the generator creates.</typeparam>
    public static PersonSpecification FromGenerator<TPerson>(IPersonGenerator<TPerson> generator)
        where TPerson : PersonBase
    {
        ArgumentNullException.ThrowIfNull(generator);
        // Wrap the typed generator in a covariant interface cast
        IPersonGenerator<PersonBase> baseGenerator = generator;
        return new PersonSpecification(null, baseGenerator, generator.Count);
    }

    /// <summary>
    /// Creates a specification from a generator configuration (deprecated).
    /// </summary>
    /// <param name="generator">The generator configuration.</param>
    /// <returns>A person specification.</returns>
    [Obsolete("Use FromGenerator(IPersonGenerator<TPerson>) instead. GeneratorConfig is deprecated.")]
    public static PersonSpecification FromGenerator(GeneratorConfig generator)
    {
        ArgumentNullException.ThrowIfNull(generator);
        // Wrap the old GeneratorConfig in an adapter
        var adapter = new GeneratorConfigAdapter(generator);
        return new PersonSpecification(null, adapter, generator.Count);
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
                // For custom person types, we cannot safely duplicate instances
                // Custom types should use IPersonGenerator<TPerson> instead
                throw new InvalidOperationException(
                    $"Cannot duplicate custom person type '{_template.GetType().Name}' using template mode. " +
                    "Custom person types should use IPersonGenerator<TPerson> to generate multiple instances.");
            }
        }
        else if (_generator != null)
        {
            // Generate from generator
            foreach (var person in _generator.Generate(factorDefinitions))
                yield return person;
        }
    }

    /// <summary>
    /// Adapter to wrap old GeneratorConfig as IPersonGenerator for backward compatibility.
    /// </summary>
    private sealed class GeneratorConfigAdapter(GeneratorConfig config) : IPersonGenerator<PersonBase>
    {
        public int Count => config.Count;

        public IEnumerable<PersonBase> Generate(IEnumerable<FactorDefinition> factorDefinitions)
        {
            return config.GeneratePersons(factorDefinitions);
        }
    }
}