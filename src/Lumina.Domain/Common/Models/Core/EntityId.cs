#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Common.Models.Core;

/// <summary>
/// Base class for all entity identifiers.
/// </summary>
/// <typeparam name="TId">The type of the identifier.</typeparam>
[DebuggerDisplay("{Value}")]
public abstract class EntityId<TId> : ValueObject
{
    #region ==================================================================== PROPERTIES =================================================================================
    public TId Value { get; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Overload C-tor.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    protected EntityId(TId value)
    {
        Value = value;
    }

#pragma warning disable CS8618
    /// <summary>
    /// Default C-tor.
    /// </summary>
    protected EntityId()
    {
    }
#pragma warning restore CS8618
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    /// <summary>
    /// Customized ToString() method.
    /// </summary>
    /// <returns>Custom string value showing relevant data for current class.</returns>
    public override string? ToString()
    {
        return Value?.ToString() ?? base.ToString();
    }
    #endregion
}