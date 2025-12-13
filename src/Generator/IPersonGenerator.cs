using dotMigrata.Core.Entities;
using dotMigrata.Core.Values;

namespace dotMigrata.Generator;

/// <summary>
/// Defines a generator for creating instances of a specific person type.
/// </summary>
/// <typeparam name="TPerson">The type of person to generate, derived from <see cref="PersonBase" />.</typeparam>
/// <remarks>
///     <para>
///     Implementations define how to generate persons of a specific type with randomized or configured attributes.
///     Each person type should have its own generator implementation that knows how to create instances
///     with appropriate properties.
///     </para>
///     <para>
///     Thread Safety: Generator implementations should be thread-safe if used in parallel contexts,
///     particularly regarding random number generation.
///     </para>
/// </remarks>
public interface IPersonGenerator<out TPerson> where TPerson : PersonBase
{
    /// <summary>
    /// Gets the number of persons this generator will create.
    /// </summary>
    int Count { get; }

    /// <summary>
    /// Generates persons according to the generator's configuration.
    /// </summary>
    /// <param name="factorDefinitions">
    /// The factor definitions for the world. Used to generate factor sensitivities for each person.
    /// </param>
    /// <returns>A collection of generated persons of type <typeparamref name="TPerson" />.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="factorDefinitions" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="factorDefinitions" /> is empty.
    /// </exception>
    /// <remarks>
    /// This method may be called multiple times and should produce consistent results if seeded.
    /// </remarks>
    IEnumerable<TPerson> Generate(IEnumerable<FactorDefinition> factorDefinitions);
}