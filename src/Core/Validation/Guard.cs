using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace dotMigrata.Core.Validation;

/// <summary>
/// Provides guard clauses for parameter validation.
/// </summary>
public static class Guard
{
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the value is null.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static void ThrowIfNull<T>(
        [NotNull] T? value,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null) => ArgumentNullException.ThrowIfNull(value, paramName);


    /// <summary>
    /// Throws an <see cref="ArgumentException"/> if the string is null or whitespace.
    /// </summary>
    /// <param name="value">The string to check.</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="value"/> is null or whitespace.</exception>
    public static void ThrowIfNullOrWhiteSpace(
        [NotNull] string? value,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null) => ArgumentException.ThrowIfNullOrWhiteSpace(value, paramName);


    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the value is not in the specified range.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="max">The maximum allowed value (inclusive).</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is outside the specified range.</exception>
    public static void ThrowIfNotInRange<T>(
        T value, T min, T max,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(paramName, value, $"Value must be between {min} and {max}.");
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the value is less than the minimum.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum allowed value (inclusive).</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than <paramref name="min"/>.</exception>
    public static void ThrowIfLessThan<T>(
        T value, T min,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0)
            throw new ArgumentOutOfRangeException(paramName, value, $"Value must be greater than or equal to {min}.");
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the value is less than or equal to the minimum.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="min">The minimum allowed value (exclusive).</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is less than or equal to <paramref name="min"/>.</exception>
    public static void ThrowIfLessThanOrEqual<T>(
        T value, T min,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(min) <= 0)
            throw new ArgumentOutOfRangeException(paramName, value, $"Value must be greater than {min}.");
    }

    /// <summary>
    /// Throws an <see cref="ArgumentOutOfRangeException"/> if the value is greater than the maximum.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to check.</param>
    /// <param name="max">The maximum allowed value (inclusive).</param>
    /// <param name="paramName">The name of the parameter.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="value"/> is greater than <paramref name="max"/>.</exception>
    public static void ThrowIfGreaterThan<T>(
        T value, T max,
        [CallerArgumentExpression(nameof(value))]
        string? paramName = null)
        where T : IComparable<T>
    {
        if (value.CompareTo(max) > 0)
            throw new ArgumentOutOfRangeException(paramName, value, $"Value must be less than or equal to {max}.");
    }
}