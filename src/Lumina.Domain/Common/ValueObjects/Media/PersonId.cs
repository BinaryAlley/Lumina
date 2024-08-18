#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
#endregion

namespace Lumina.Domain.Common.ValueObjects.Media;

/// <summary>
/// Value Object for the Id of a person.
/// </summary>
public sealed class PersonId : ValueObject
{
    #region ==================================================================== PROPERTIES =================================================================================
    public Guid Value { get; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Overload C-tor.
    /// </summary>
    /// <param name="value">The value representing this object.</param>
    private PersonId(Guid value)
    {
        Value = value;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of <see cref="PersonId"/>.
    /// </summary>
    /// <returns>The created <see cref="PersonId"/> instance.</returns>
    public static PersonId CreateUnique()
    {
        // TODO: enforce invariants
        return new PersonId(Guid.NewGuid()); 
    }

    /// <summary>
    /// Creates a new instance of <see cref="PersonId"/> from <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value used to create the <see cref="PersonId"/> instance.</param>
    /// <returns>The created <see cref="PersonId"/> instance.</returns>
    public static PersonId Create(Guid value)
    {
        // TODO: enforce invariants
        return new PersonId(value); 
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    #endregion
}