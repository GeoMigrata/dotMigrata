using dotMigrata.Core.Entities;
using dotMigrata.Core.Values;
using dotMigrata.Generator;

namespace dotMigrata.Snapshot.Interfaces;

/// <summary>
/// Converts between person model classes and runtime person instances.
/// Uses Dependency Injection pattern for custom person type support.
/// </summary>
/// <typeparam name="TPersonModel">The person model class implementing <see cref="IPersonModel" />.</typeparam>
/// <typeparam name="TPerson">The runtime person class derived from <see cref="PersonBase" />.</typeparam>
/// <typeparam name="TGenerator">The generator class implementing <see cref="IPersonGenerator{TPerson}" />.</typeparam>
/// <remarks>
/// This interface enables type-safe conversion between XML models and runtime instances.
/// Implement this interface to support custom person types in snapshots.
/// </remarks>
public interface IPersonModelConverter<TPersonModel, TPerson, TGenerator>
    where TPersonModel : IPersonModel
    where TPerson : PersonBase
    where TGenerator : IPersonGenerator<TPerson>
{
    /// <summary>
    /// Creates a person instance from a person model (template mode).
    /// </summary>
    /// <param name="model">The person model containing fixed property values.</param>
    /// <param name="factorLookup">Dictionary mapping factor IDs to FactorDefinition instances.</param>
    /// <param name="allFactors">All factor definitions in the world.</param>
    /// <returns>A new person instance.</returns>
    TPerson CreatePerson(
        TPersonModel model,
        Dictionary<string, FactorDefinition> factorLookup,
        List<FactorDefinition> allFactors);

    /// <summary>
    /// Creates a generator instance from a person model (generator mode).
    /// </summary>
    /// <param name="model">The person model containing generator specifications.</param>
    /// <param name="factorLookup">Dictionary mapping factor IDs to FactorDefinition instances.</param>
    /// <param name="allFactors">All factor definitions in the world.</param>
    /// <returns>A new generator instance.</returns>
    TGenerator CreateGenerator(
        TPersonModel model,
        Dictionary<string, FactorDefinition> factorLookup,
        List<FactorDefinition> allFactors);

    /// <summary>
    /// Converts a person instance to a person model for serialization.
    /// </summary>
    /// <param name="person">The person instance to convert.</param>
    /// <returns>A person model ready for XML serialization.</returns>
    TPersonModel ToModel(TPerson person);

    /// <summary>
    /// Converts a generator instance to a person model for serialization.
    /// </summary>
    /// <param name="generator">The generator instance to convert.</param>
    /// <returns>A person model ready for XML serialization.</returns>
    TPersonModel ToModel(TGenerator generator);
}