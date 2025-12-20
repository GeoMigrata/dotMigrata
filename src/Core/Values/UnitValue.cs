using System.Globalization;

namespace dotMigrata.Core.Values;

/// <summary>
/// Represents a normalized value guaranteed to be between 0 and 1.
/// Used for factor intensities, sensitivities, and other normalized measurements.
/// </summary>
/// <remarks>
/// This type ensures type safety by clamping values to the valid range.
/// All arithmetic operations automatically maintain range validity.
/// Implemented as a readonly record struct for optimal performance and value semantics.
/// </remarks>
public readonly record struct UnitValue : IComparable<UnitValue>
{
    /// <summary>
    /// Initializes a new instance, clamping the value to valid range.
    /// </summary>
    /// <param name="value">The value to normalize. Will be clamped if needed.</param>
    /// <exception cref="ArgumentException">Thrown when value is NaN or Infinity.</exception>
    private UnitValue(double value)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentException("Value must be a finite number.", nameof(value));

        Value = Math.Clamp(value, 0.0, 1.0);
    }

    /// <summary>
    /// Gets the underlying value.
    /// </summary>
    public double Value { get; }

    #region IComparable

    /// <summary>
    /// Compares this instance with another <see cref="UnitValue" />.
    /// </summary>
    public int CompareTo(UnitValue other) => Value.CompareTo(other.Value);

    #endregion

    /// <summary>
    /// Returns a string representation of this value.
    /// </summary>
    public override string ToString() => Value.ToString("F2", CultureInfo.InvariantCulture);

    /// <summary>
    /// Returns a string representation of this value with specified format.
    /// </summary>
    public string ToString(string format) => Value.ToString(format, CultureInfo.InvariantCulture);

    #region Factory Methods

    /// <summary>
    /// Creates a <see cref="UnitValue" /> from a ratio value.
    /// </summary>
    /// <param name="ratio">The ratio value to use. Values outside the valid range will be clamped.</param>
    /// <returns>A new <see cref="UnitValue" /> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when ratio is NaN or Infinity.</exception>
    public static UnitValue FromRatio(double ratio) => new(ratio);

    /// <summary>
    /// Attempts to create a <see cref="UnitValue" /> from a ratio without throwing an exception.
    /// </summary>
    /// <param name="ratio">The ratio value.</param>
    /// <param name="result">The resulting normalized value if successful.</param>
    /// <returns><c>true</c> if conversion succeeded; otherwise, <c>false</c>.</returns>
    public static bool TryFromRatio(double ratio, out UnitValue result)
    {
        if (double.IsNaN(ratio) || double.IsInfinity(ratio))
        {
            result = default;
            return false;
        }

        result = new UnitValue(ratio);
        return true;
    }

    /// <summary>
    /// Creates a <see cref="UnitValue" /> from a percentage (0-100).
    /// </summary>
    /// <param name="percentage">The percentage value. Will be divided by 100 and clamped if needed.</param>
    /// <returns>A new <see cref="UnitValue" /> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when percentage is NaN or Infinity.</exception>
    public static UnitValue FromPercentage(double percentage)
    {
        if (double.IsNaN(percentage) || double.IsInfinity(percentage))
            throw new ArgumentException("Percentage must be a finite number.", nameof(percentage));

        return new UnitValue(percentage / 100.0);
    }

    /// <summary>
    /// Attempts to create a <see cref="UnitValue" /> from a percentage without throwing an exception.
    /// </summary>
    /// <param name="percentage">The percentage value (0-100).</param>
    /// <param name="result">The resulting normalized value if successful.</param>
    /// <returns><c>true</c> if conversion succeeded; otherwise, <c>false</c>.</returns>
    public static bool TryFromPercentage(double percentage, out UnitValue result)
    {
        if (double.IsNaN(percentage) || double.IsInfinity(percentage))
        {
            result = default;
            return false;
        }

        result = new UnitValue(percentage / 100.0);
        return true;
    }

    #endregion

    #region Common Values

    /// <summary>Gets a normalized value of 0.0 (minimum).</summary>
    public static UnitValue Zero => new(0.0);

    /// <summary>Gets a normalized value of 0.5 (middle).</summary>
    public static UnitValue Half => new(0.5);

    /// <summary>Gets a normalized value of 1.0 (maximum).</summary>
    public static UnitValue One => new(1.0);

    /// <summary>Gets a normalized value of 0.25 (quarter).</summary>
    public static UnitValue Quarter => new(0.25);

    /// <summary>Gets a normalized value of 0.75 (three quarters).</summary>
    public static UnitValue ThreeQuarters => new(0.75);

    #endregion

    #region Arithmetic Operators

    /// <summary>
    /// Adds two normalized values, clamping the result if needed.
    /// </summary>
    public static UnitValue operator +(UnitValue left, UnitValue right) => new(left.Value + right.Value);

    /// <summary>
    /// Subtracts two normalized values, clamping the result if needed.
    /// </summary>
    public static UnitValue operator -(UnitValue left, UnitValue right) => new(left.Value - right.Value);

    /// <summary>
    /// Multiplies two normalized values, clamping the result if needed.
    /// </summary>
    public static UnitValue operator *(UnitValue left, UnitValue right) => new(left.Value * right.Value);

    /// <summary>
    /// Multiplies a normalized value by a scalar, clamping the result to if needed.
    /// </summary>
    public static UnitValue operator *(UnitValue left, double right) => new(left.Value * right);

    /// <summary>
    /// Multiplies a scalar by a normalized value, clamping the result if needed.
    /// </summary>
    public static UnitValue operator *(double left, UnitValue right) => new(left * right.Value);

    /// <summary>
    /// Divides a normalized value by a scalar, clamping the result to if needed.
    /// </summary>
    public static UnitValue operator /(UnitValue left, double right) =>
        right == 0.0
            ? throw new DivideByZeroException("Cannot divide by zero.")
            : new UnitValue(left.Value / right);

    #endregion

    #region Comparison Operators

    /// <summary>
    /// Determines whether one normalized value is less than another.
    /// </summary>
    public static bool operator <(UnitValue left, UnitValue right) => left.Value < right.Value;

    /// <summary>
    /// Determines whether one normalized value is greater than another.
    /// </summary>
    public static bool operator >(UnitValue left, UnitValue right) => left.Value > right.Value;

    /// <summary>
    /// Determines whether one normalized value is less than or equal to another.
    /// </summary>
    public static bool operator <=(UnitValue left, UnitValue right) => left.Value <= right.Value;

    /// <summary>
    /// Determines whether one normalized value is greater than or equal to another.
    /// </summary>
    public static bool operator >=(UnitValue left, UnitValue right) => left.Value >= right.Value;

    #endregion

    #region Conversions

    /// <summary>
    /// Implicitly converts a <see cref="UnitValue" /> to a <see cref="double" />.
    /// </summary>
    public static implicit operator double(UnitValue value) => value.Value;

    /// <summary>
    /// Explicitly converts a <see cref="double" /> to a <see cref="UnitValue" />, clamping to if needed.
    /// </summary>
    public static explicit operator UnitValue(double value) => FromRatio(value);

    #endregion

    #region Utility Methods

    /// <summary>
    /// Converts this value to a percentage (0-100).
    /// </summary>
    public double ToPercentage() => Value * 100.0;

    /// <summary>
    /// Linearly interpolates between two normalized values.
    /// </summary>
    /// <param name="a">The start value.</param>
    /// <param name="b">The end value.</param>
    /// <param name="t">The interpolation factor (0-1).</param>
    /// <returns>The interpolated value.</returns>
    public static UnitValue Lerp(UnitValue a, UnitValue b, double t) =>
        new(a.Value + (b.Value - a.Value) * Math.Clamp(t, 0.0, 1.0));

    /// <summary>
    /// Returns the minimum of two normalized values.
    /// </summary>
    public static UnitValue Min(UnitValue a, UnitValue b) => new(Math.Min(a.Value, b.Value));

    /// <summary>
    /// Returns the maximum of two normalized values.
    /// </summary>
    public static UnitValue Max(UnitValue a, UnitValue b) => new(Math.Max(a.Value, b.Value));

    #endregion
}