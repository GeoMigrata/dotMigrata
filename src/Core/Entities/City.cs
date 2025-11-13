using dotGeoMigrata.Core.Values;
using System.Collections.Concurrent;

namespace dotGeoMigrata.Core.Entities;

/// <summary>
/// Represents a city with geographic location, factors, and individual persons.
/// </summary>
public class City
{
    private readonly double _area;
    private readonly Dictionary<FactorDefinition, FactorValue> _factorLookup;
    private readonly List<FactorValue> _factorValues;
    private readonly ConcurrentDictionary<string, Person> _persons;

    /// <summary>
    /// Initializes a new instance of the City class.
    /// </summary>
    /// <param name="factorValues">Optional initial factor values.</param>
    /// <param name="persons">Optional initial persons residing in this city.</param>
    public City(
        IEnumerable<FactorValue>? factorValues = null,
        IEnumerable<Person>? persons = null)
    {
        _factorValues = factorValues?.ToList() ?? [];
        _factorLookup = _factorValues.ToDictionary(fv => fv.Definition, fv => fv);

        _persons = new ConcurrentDictionary<string, Person>();
        if (persons == null) return;
        foreach (var person in persons)
        {
            _persons[person.Id] = person;
            person.CurrentCity = this;
        }
    }

    /// <summary>
    /// Gets or initializes the display name of the city.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets or initializes the area of the city in square kilometers.
    /// Must be greater than 0.
    /// </summary>
    public double Area
    {
        get => _area;
        init => _area = value > 0 ? value : throw new ArgumentException("Area must be greater than 0", nameof(value));
    }

    /// <summary>
    /// Gets or initializes the geographic position of the city.
    /// </summary>
    public required Coordinate Location { get; init; }

    /// <summary>
    /// Gets or initializes the maximum population capacity of the city.
    /// Represents the upper limit of residents the city can accommodate.
    /// A value of null or 0 indicates no explicit capacity limit.
    /// </summary>
    public int? Capacity { get; init; }

    /// <summary>
    /// Gets the read-only list of factor values for this city.
    /// </summary>
    public IReadOnlyList<FactorValue> FactorValues => _factorValues;

    /// <summary>
    /// Gets the read-only collection of persons residing in this city.
    /// </summary>
    public IReadOnlyCollection<Person> Persons => _persons.Values.ToList().AsReadOnly();

    /// <summary>
    /// Gets the total population count (number of persons) in this city.
    /// </summary>
    public int Population => _persons.Count;

    /// <summary>
    /// Updates the intensity of an existing FactorValue for the specified factor definition.
    /// This keeps FactorValue immutable but allows controlled updates via City API.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when factor is null.</exception>
    /// <exception cref="ArgumentException">Thrown when factor has no matched value in this city.</exception>
    public void UpdateFactorIntensity(FactorDefinition factor, double newIntensity)
    {
        ArgumentNullException.ThrowIfNull(factor);

        if (!_factorLookup.TryGetValue(factor, out var factorValue))
            throw new ArgumentException("Given factor has no matched value in this city.", nameof(factor));

        factorValue.Intensity = newIntensity;
    }

    /// <summary>
    /// Tries to get the factor value for the specified factor definition.
    /// </summary>
    /// <param name="factor">The factor definition to look up.</param>
    /// <param name="factorValue">The factor value if found.</param>
    /// <returns>True if the factor value exists, false otherwise.</returns>
    public bool TryGetFactorValue(FactorDefinition factor, out FactorValue? factorValue)
    {
        ArgumentNullException.ThrowIfNull(factor);
        return _factorLookup.TryGetValue(factor, out factorValue);
    }

    /// <summary>
    /// Adds a person to this city.
    /// </summary>
    /// <param name="person">The person to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when person is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a person with the same ID already exists in this city.</exception>
    public void AddPerson(Person person)
    {
        ArgumentNullException.ThrowIfNull(person, nameof(person));

        if (!_persons.TryAdd(person.Id, person))
            throw new InvalidOperationException(
                $"Person with ID '{person.Id}' already exists in city '{DisplayName}'.");

        person.CurrentCity = this;
    }

    /// <summary>
    /// Removes a person from this city.
    /// </summary>
    /// <param name="person">The person to remove.</param>
    /// <exception cref="ArgumentNullException">Thrown when person is null.</exception>
    /// <returns>True if the person was removed, false if the person was not in this city.</returns>
    public bool RemovePerson(Person person)
    {
        ArgumentNullException.ThrowIfNull(person, nameof(person));

        if (!_persons.TryRemove(person.Id, out _)) return false;
        if (person.CurrentCity == this)
            person.CurrentCity = null;
        return true;
    }

    /// <summary>
    /// Tries to get a person by their ID.
    /// </summary>
    /// <param name="personId">The person's unique identifier.</param>
    /// <param name="person">The person if found.</param>
    /// <returns>True if the person was found, false otherwise.</returns>
    public bool TryGetPerson(string personId, out Person? person)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(personId, nameof(personId));
        return _persons.TryGetValue(personId, out person);
    }
}