using dotGeoMigrata.Core.Entities;
using dotGeoMigrata.Core.Values;

namespace dotGeoMigrata.Generator;

/// <summary>
/// A collection of person specifications that can be used to generate a population.
/// Supports adding individual person instances, duplicates, and generators.
/// </summary>
public sealed class PersonCollection
{
    private readonly List<PersonSpecification> _specifications = [];

    /// <summary>
    /// Gets the number of specifications in this collection.
    /// </summary>
    public int SpecificationCount => _specifications.Count;

    /// <summary>
    /// Adds a single person instance to this collection.
    /// </summary>
    /// <param name="person">The person to add.</param>
    /// <returns>This collection for method chaining.</returns>
    public PersonCollection Add(Person person)
    {
        ArgumentNullException.ThrowIfNull(person);
        _specifications.Add(PersonSpecification.FromPerson(person));
        return this;
    }

    /// <summary>
    /// Adds multiple duplicates of a person to this collection.
    /// </summary>
    /// <param name="person">The person template to duplicate.</param>
    /// <param name="count">Number of duplicates to create.</param>
    /// <returns>This collection for method chaining.</returns>
    public PersonCollection Add(Person person, int count)
    {
        ArgumentNullException.ThrowIfNull(person);
        _specifications.Add(PersonSpecification.FromPerson(person, count));
        return this;
    }

    /// <summary>
    /// Adds a generator configuration to this collection.
    /// </summary>
    /// <param name="generator">The generator configuration.</param>
    /// <returns>This collection for method chaining.</returns>
    public PersonCollection Add(GeneratorConfig generator)
    {
        ArgumentNullException.ThrowIfNull(generator);
        _specifications.Add(PersonSpecification.FromGenerator(generator));
        return this;
    }

    /// <summary>
    /// Generates all persons from all specifications in this collection.
    /// </summary>
    /// <param name="factorDefinitions">The factor definitions to use for person generation.</param>
    /// <returns>A collection of all generated persons.</returns>
    public IEnumerable<Person> GenerateAllPersons(IEnumerable<FactorDefinition> factorDefinitions)
    {
        ArgumentNullException.ThrowIfNull(factorDefinitions);

        var factors = factorDefinitions.ToList();
        if (factors.Count == 0)
            throw new ArgumentException("Must provide at least one factor definition.", nameof(factorDefinitions));

        foreach (var specification in _specifications)
        foreach (var person in specification.GeneratePersons(factors))
            yield return person;
    }

    /// <summary>
    /// Gets the total count of persons that will be generated from all specifications.
    /// </summary>
    /// <returns>Total person count.</returns>
    public int GetTotalCount()
    {
        return _specifications.Sum(spec => spec.Count);
    }

    /// <summary>
    /// Clears all specifications from this collection.
    /// </summary>
    public void Clear()
    {
        _specifications.Clear();
    }
}