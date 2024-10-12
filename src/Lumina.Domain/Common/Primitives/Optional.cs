namespace Lumina.Domain.Common.Primitives;

/// <summary>
/// Primitive struct for explicitly handling cases when a value might be unknown or not applicable, 
/// distinguishing between "not set" and "intentionally empty".
/// </summary>
/// <typeparam name="TValue">The type of the value represented by this structure.</typeparam>
public readonly struct Optional<TValue>
{
    private readonly TValue? _value;

    /// <summary>
    /// Gets a value indicating whether this structure has a value.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// Gets the value represented by this structure.
    /// </summary>
    public TValue Value => HasValue ? _value! : default!;

    /// <summary>
    /// Initializes a new instance of the <see cref="Optional{TValue}"/> structure.
    /// </summary>
    /// <param name="value">The value represented by this structure.</param>
    /// <param name="hasValue"><see langword="true"/> if <paramref name="value"/> is not <see langword="null"/>, <see langword="false"/>otherwise.</param>
    private Optional(TValue? value, bool hasValue)
    {
        _value = value;
        HasValue = hasValue;
    }

    /// <summary>
    /// Custom implementation operator for converting <paramref name="value"/> to an <see cref="Optional{TValue}"/>.
    /// </summary>
    /// <param name="value">The value to be converted.</param>
    public static implicit operator Optional<TValue>(TValue value)
    {
        return Some(value);
    }

    /// <summary>
    /// Creates an <see cref="Optional{TValue}"/> with a value.
    /// </summary>
    /// <param name="value">The value to wrap in the <see cref="Optional{TValue}"/>.</param>
    /// <returns>An <see cref="Optional{TValue}"/> containing the specified value.</returns>
    public static Optional<TValue> Some(TValue value)
    {
        return new Optional<TValue>(value, true);
    }

    /// <summary>
    /// Creates an <see cref="Optional{TValue}"/> with no value.
    /// </summary>
    /// <returns>An empty <see cref="Optional{TValue}"/>.</returns>
    public static Optional<TValue> None()
    {
        return new(default, false);
    }

    /// <summary>
    /// Creates an <see cref="Optional{TValue}"/> from a nullable reference type.
    /// </summary>
    /// <param name="value">The nullable value to convert.</param>
    /// <returns>An <see cref="Optional{TValue}"/> representing the nullable value.</returns>
    public static Optional<TValue> FromNullable(TValue? value)
    {
        return value is null ? None() : Some(value);
    }

    /// <summary>
    /// Creates an <see cref="Optional{TValue}"/> from a nullable value type.
    /// </summary>
    /// <typeparam name="TNullableValue">The underlying value type.</typeparam>
    /// <param name="value">The nullable value to convert.</param>
    /// <returns>An <see cref="Optional{TValue}"/> representing the nullable value.</returns>
    public static Optional<TValue> FromNullable<TNullableValue>(TNullableValue? value) where TNullableValue : struct, TValue
    {
        return value.HasValue ? Some(value.Value) : None();
    }

    /// <summary>
    /// Customized ToString() method.
    /// </summary>
    /// <returns>Custom string value showing relevant data for current class.</returns>
    public override string ToString()
    {
        return HasValue ? Value?.ToString()! : string.Empty;
    }
}