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
    public bool HasValue { get; }
    public T Value
    {
        get
        {
            return HasValue ? _value! : throw new InvalidOperationException("Optional has no value.");
        }
    }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Overload C-tor.
    /// </summary>
    /// <param name="value">The value represented by this structure.</param>
    public Optional(T value)
    {
        _value = value;
        HasValue = true;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Custom implementation operator for converting <paramref name="value"/> to an <see cref="Optional{T}"/>.
    /// </summary>
    /// <param name="value">The value to be converted.</param>
    public static implicit operator Optional<T>(T value)
    {
        return new Optional<T>(value);
    }
    #endregion
}