#region ========================================================================= USING =====================================================================================
using ErrorOr;
using Lumina.Domain.Common.Enums;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
#endregion

namespace Lumina.Domain.Common.ValueObjects.Media;

/// <summary>
/// Value Object for a contributor of a media element.
/// </summary>
public sealed class MediaContributor : ValueObject
{
    #region ==================================================================== PROPERTIES =================================================================================
    public PersonId PersonId { get; }
    public Optional<MediaContributorRole> Role { get; }
    #endregion

    #region ====================================================================== CTOR =====================================================================================
    /// <summary>
    /// Overload C-tor.
    /// </summary>
    /// <param name="personId">The id of the person represented by this object.</param>
    /// <param name="role">The role of the person represented by this object.</param>
    private MediaContributor(PersonId personId, Optional<MediaContributorRole> role)
    {
        Role = role;
        PersonId = personId;
    }
    #endregion

    #region ===================================================================== METHODS ===================================================================================
    /// <summary>
    /// Creates a new instance of <see cref="MediaContributor"/>.
    /// </summary>
    /// <param name="personId">The id of the person represented by this object.</param>
    /// <param name="role">The role of the person represented by this object.</param>
    /// <returns>
    /// An <see cref="ErrorOr{T}"/> containing either a successfully created <see cref="MediaContributor"/> or an error message.
    /// </returns>
    public static ErrorOr<MediaContributor> Create(PersonId personId, Optional<MediaContributorRole> role)
    {
        return new MediaContributor(personId, role);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return PersonId;
        yield return Role;
    }
    #endregion
}