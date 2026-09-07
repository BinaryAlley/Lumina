#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.MediaContributorBoundedContext.MediaContributorAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for the credit of a media contributor in a music media element, carrying the role the contributor played.
/// The role carries both a free-form display name, as returned by the metadata providers, and the canonical category
/// the display name normalizes to, so that roles describing the same kind of contribution are never treated as distinct.
/// </summary>
[DebuggerDisplay("{RoleDisplayName}")]
public sealed class MediaContributorCredit : ValueObject
{
    /// <summary>
    /// Gets the unique identifier of the credited media contributor.
    /// </summary>
    public MediaContributorId ContributorId { get; }

    /// <summary>
    /// Gets the display name of the role the contributor played.
    /// </summary>
    public string RoleDisplayName { get; }

    /// <summary>
    /// Gets the canonical category of the role the contributor played.
    /// </summary>
    public MediaContributorRoleCategory RoleCategory { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaContributorCredit"/> class.
    /// </summary>
    /// <param name="contributorId">The unique identifier of the credited media contributor.</param>
    /// <param name="roleDisplayName">The display name of the role the contributor played.</param>
    /// <param name="roleCategory">The canonical category of the role the contributor played.</param>
    private MediaContributorCredit(MediaContributorId contributorId, string roleDisplayName, MediaContributorRoleCategory roleCategory)
    {
        ContributorId = contributorId;
        RoleDisplayName = roleDisplayName;
        RoleCategory = roleCategory;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaContributorCredit"/> class.
    /// </summary>
    /// <param name="contributorId">The unique identifier of the credited media contributor.</param>
    /// <param name="roleDisplayName">The display name of the role the contributor played.</param>
    /// <param name="roleCategory">The canonical category of the role the contributor played.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="MediaContributorCredit"/>, or an error message.
    /// </returns>
    public static Result<MediaContributorCredit> Create(MediaContributorId contributorId, string roleDisplayName, MediaContributorRoleCategory roleCategory)
    {
        if (string.IsNullOrWhiteSpace(roleDisplayName))
            return Errors.Music.CreditRoleDisplayNameCannotBeEmpty;

        return new MediaContributorCredit(contributorId, roleDisplayName, roleCategory);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return ContributorId;
        yield return RoleDisplayName;
        yield return RoleCategory;
    }
}
