namespace Lumina.Domain.Common.Primitives;

/// <summary>
/// Primitive struct for explicitly handling cases when a value might be unknown or not applicable, 
/// distinguishing between "not set" and "intentionally empty".
/// </summary>
/// <typeparam name="T">The type of the value represented by this structure.</typeparam>
public readonly struct Optional<T>
{
    #region ================================================================== FIELD MEMBERS ================================================================================
    private readonly T? _value;
    #endregion

    #region ==================================================================== PROPERTIES =================================================================================
    /// <summary>
    /// Gets a value indicating whether this structure has a value.
    /// </summary>
    public bool HasValue { get; }
    
    /// <summary>
    /// Gets the value represented by this structure.
    /// </summary>
    public T Value
    {
        get
        {
            return HasValue ? _value! : default!;
        }
    }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Initializes a new instance of the <see cref="Optional{T}"/> structure.
    /// </summary>
    /// <param name="value">The value represented by this structure.</param>
    /// <param name="hasValue"><see langword="true"/> if <paramref name="value"/> is not <see langword="null"/>, <see langword="false"/>otherwise.</param>
    private Optional(T? value, bool hasValue)
    {
        _value = value;
        HasValue = hasValue;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Custom implementation operator for converting <paramref name="value"/> to an <see cref="Optional{T}"/>.
    /// </summary>
    /// <param name="value">The value to be converted.</param>
    public static implicit operator Optional<T>(T value)
    {
        return Some(value);
    }

    /// <summary>
    /// Creates an <see cref="Optional{T}"/> with a value.
    /// </summary>
    /// <param name="value">The value to wrap in the <see cref="Optional{T}"/>.</param>
    /// <returns>An <see cref="Optional{T}"/> containing the specified value.</returns>
    public static Optional<T> Some(T value)
    {
        return new Optional<T>(value, true);
    }

    /// <summary>
    /// Creates an <see cref="Optional{T}"/> with no value.
    /// </summary>
    /// <returns>An empty <see cref="Optional{T}"/>.</returns>
    public static Optional<T> None()
    {
        return new(default, false);
    }

    /// <summary>
    /// Creates an <see cref="Optional{T}"/> from a nullable reference type.
    /// </summary>
    /// <param name="value">The nullable value to convert.</param>
    /// <returns>An <see cref="Optional{T}"/> representing the nullable value.</returns>
    public static Optional<T> FromNullable(T? value)
    {
        return value is null ? None() : Some(value);
    }

    /// <summary>
    /// Creates an <see cref="Optional{T}"/> from a nullable value type.
    /// </summary>
    /// <typeparam name="TValue">The underlying value type.</typeparam>
    /// <param name="value">The nullable value to convert.</param>
    /// <returns>An <see cref="Optional{T}"/> representing the nullable value.</returns>
    public static Optional<T> FromNullable<TValue>(TValue? value) where TValue : struct, T
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
    #endregion
}