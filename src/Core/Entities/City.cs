using System.Diagnostics.CodeAnalysis;
using dotMigrata.Core.Values;

namespace dotMigrata.Core.Entities;

/// <summary>
/// Represents a city with geographic location, factors, and individual persons.
/// <remarks>
///     <para>Uses reference-based person management with <see cref="HashSet{T}" /> for O(1) operations.</para>
///     <para>Implements <see cref="IDisposable" /> for proper cleanup of synchronization resources.</para>
/// </remarks>
/// </summary>
public sealed class City : IDisposable
{
    private readonly List<FactorIntensity> _factorIntensities;
    private readonly Dictionary<FactorDefinition, FactorIntensity> _factorLookup;
    private readonly HashSet<PersonBase> _persons;
    private readonly ReaderWriterLockSlim _personsLock = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="City" /> class.
    /// </summary>
    /// <param name="factorIntensities">The initial factor values for the city, or <see langword="null" /> for none.</param>
    /// <param name="persons">The initial persons residing in this city, or <see langword="null" /> for none.</param>
    public City(
        IEnumerable<FactorIntensity>? factorIntensities = null,
        IEnumerable<PersonBase>? persons = null)
    {
        _factorIntensities = factorIntensities?.ToList() ?? [];
        _factorLookup = _factorIntensities.ToDictionary(fi => fi.Definition, fv => fv);

        _persons = persons?.ToHashSet() ?? [];
        foreach (var person in _persons)
            person.CurrentCity = this;
    }

    /// <summary>
    /// Gets the display name of the city.
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets the area of the city in square kilometers.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when attempting to set a value less than or equal to zero.
    /// </exception>
    public double Area
    {
        get;
        init => field = value > 0 ? value : throw new ArgumentException("Area must be greater than 0", nameof(value));
    }

    /// <summary>
    /// Gets the geographic position of the city.
    /// </summary>
    public required Coordinate Location { get; init; }

    /// <summary>
    /// Gets the maximum population capacity of the city.
    /// </summary>
    /// <remarks>
    /// Represents the upper limit of residents the city can accommodate.
    /// A value of <see langword="null" /> or 0 indicates no explicit capacity limit.
    /// </remarks>
    public int? Capacity { get; init; }

    /// <summary>
    /// Gets the read-only list of factor intensities for this city.
    /// </summary>
    public IReadOnlyList<FactorIntensity> FactorIntensities => _factorIntensities;

    /// <summary>
    /// Gets the read-only collection of persons residing in this city.
    /// </summary>
    /// <remarks>
    /// Returns a snapshot to avoid holding locks during iteration.
    /// </remarks>
    public IReadOnlyList<PersonBase> Persons
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
    /// Releases the resources used by this <see cref="City" />.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }

    /// <summary>
    /// Updates the intensity of an existing factor in this city.
    /// </summary>
    /// <param name="factorIntensity">The new factor intensity to apply. Must already exist in this city.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factor" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="factorIntensity" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    /// Intensities are immutable <see cref="UnitValue" /> values.
    /// This method replaces the entire FactorIntensity instance with the provided one.
    /// </remarks>
    public void UpdateFactorIntensity(FactorIntensity factorIntensity)
    {
        ArgumentNullException.ThrowIfNull(factorIntensity);

        if (!_factorLookup.ContainsKey(factorIntensity.Definition))
            throw new ArgumentException("Given factor has no matched intensity in this city.", nameof(factorIntensity));

        _factorLookup[factorIntensity.Definition] = factorIntensity;

        var index = _factorIntensities.FindIndex(fi => fi.Definition == factorIntensity.Definition);
        if (index >= 0)
            _factorIntensities[index] = factorIntensity;
    }

    /// <summary>
    /// Attempts to get the factor intensity for the specified factor definition.
    /// </summary>
    /// <param name="factor">The factor definition to look up.</param>
    /// <param name="factorIntensity">
    /// When this method returns, contains the factor intensity if found;
    /// otherwise, <see langword="null" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> if the factor intensity exists; otherwise, <see langword="false" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factor" /> is <see langword="null" />.
    /// </exception>
    public bool TryGetFactorIntensity(FactorDefinition factor, [NotNullWhen(true)] out FactorIntensity? factorIntensity)
    {
        ArgumentNullException.ThrowIfNull(factor);
        return _factorLookup.TryGetValue(factor, out factorIntensity);
    }

    /// <summary>
    /// Adds a person to this city.
    /// </summary>
    /// <param name="person">The person to add.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="person" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the person already exists in this city.
    /// </exception>
    /// <remarks>
    /// Thread-safe O(1) operation using <see cref="HashSet{T}" />.
    /// </remarks>
    public void AddPerson(PersonBase person)
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
    /// Removes a person from this city.
    /// </summary>
    /// <param name="person">The person to remove.</param>
    /// <returns>
    /// <see langword="true" /> if the person was removed;
    /// <see langword="false" /> if the person was not in this city.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="person" /> is <see langword="null" />.
    /// </exception>
    /// <remarks>
    /// Thread-safe O(1) operation using <see cref="HashSet{T}" />.
    /// </remarks>
    public bool RemovePerson(PersonBase person)
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

    /// <summary>
    /// Gets the population ratio relative to capacity.
    /// </summary>
    /// <returns>
    /// The ratio of current population to capacity as a <see cref="UnitValue" />, or <see cref="UnitValue.One" /> if capacity
    /// is not set.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///     This provides a consistent type-safe way to calculate population pressure across feedback strategies.
    ///     </para>
    ///     <para>
    ///     Returns <see cref="UnitValue.One" /> when capacity is null or zero to indicate no capacity constraint.
    ///     A value of 1.0 represents a city at equilibrium with no pressure from capacity limits,
    ///     allowing feedback strategies to apply baseline adjustments.
    ///     </para>
    /// </remarks>
    public UnitValue GetPopulationRatio()
    {
        if (Capacity is null or 0)
            return UnitValue.One; // No capacity constraint - treat as equilibrium

        var ratio = (double)Population / Capacity.Value;
        return UnitValue.FromRatio(ratio);
    }

    /// <summary>
    /// Releases the unmanaged resources used by this <see cref="City" />
    /// and optionally releases managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true" /> to release both managed and unmanaged resources;
    /// <see langword="false" /> to release only unmanaged resources.
    /// </param>
    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
            _personsLock.Dispose();

        _disposed = true;
    }
}