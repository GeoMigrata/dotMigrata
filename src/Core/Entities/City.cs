using dotMigrata.Core.Values;

namespace dotMigrata.Core.Entities;

/// <summary>
/// Represents a city with geographic location, factors, and individual persons.
/// Uses reference-based person management with HashSet for O(1) operations.
/// </summary>
public class City
{
    private readonly Dictionary<FactorDefinition, FactorValue> _factorLookup;
    private readonly List<FactorValue> _factorValues;
    private readonly HashSet<Person> _persons;
    private readonly ReaderWriterLockSlim _personsLock = new();

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

        _persons = [];
        if (persons == null) return;
        foreach (var person in persons)
        {
            _persons.Add(person);
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
        get;
        init => field = value > 0 ? value : throw new ArgumentException("Area must be greater than 0", nameof(value));
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
    /// Returns a snapshot to avoid holding locks during iteration.
    /// </summary>
    public IReadOnlyList<Person> Persons
    {
        get
        {
            _personsLock.EnterReadLock();
            try
            {
                return _persons.ToList().AsReadOnly();
            }
            finally
            {
                _personsLock.ExitReadLock();
            }
        }
    }

    /// <summary>
    /// Gets the total population count (number of persons) in this city.
    /// </summary>
    public int Population
    {
        get
        {
            _personsLock.EnterReadLock();
            try
            {
                return _persons.Count;
            }
            finally
            {
                _personsLock.ExitReadLock();
            }
        }
    }

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
    /// Adds a person to this city. Thread-safe O(1) operation using HashSet.
    /// </summary>
    /// <param name="person">The person to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when person is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the person already exists in this city.</exception>
    public void AddPerson(Person person)
    {
        ArgumentNullException.ThrowIfNull(person);

        _personsLock.EnterWriteLock();
        try
        {
            if (!_persons.Add(person))
                throw new InvalidOperationException($"Person already exists in city '{DisplayName}'.");
        }
        finally
        {
            _personsLock.ExitWriteLock();
        }

        person.CurrentCity = this;
    }

    /// <summary>
    /// Removes a person from this city. Thread-safe O(1) operation using HashSet.
    /// </summary>
    /// <param name="person">The person to remove.</param>
    /// <exception cref="ArgumentNullException">Thrown when person is null.</exception>
    /// <returns>True if the person was removed, false if the person was not in this city.</returns>
    public bool RemovePerson(Person person)
    {
        ArgumentNullException.ThrowIfNull(person);

        bool removed;
        _personsLock.EnterWriteLock();
        try
        {
            removed = _persons.Remove(person);
        }
        finally
        {
            _personsLock.ExitWriteLock();
        }

        if (removed && person.CurrentCity == this)
            person.CurrentCity = null;

        return removed;
    }
}