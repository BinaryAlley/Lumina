#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.MediaContributorBoundedContext.MediaContributorAggregate;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Value Object for a media contributor of a music media element, carrying the role the contributor played.
/// The media contributor is identified only by its Id, duplicated from the Media Contributor bounded context, and the role is
/// the canonical role the contributor played, used as the key of its localized display string.
/// </summary>
[DebuggerDisplay("{Role}")]
public sealed class MusicMediaContributor : ValueObject
{
    /// <summary>
    /// Gets the unique identifier of the media contributor.
    /// </summary>
    public MediaContributorId ContributorId { get; }

    /// <summary>
    /// Gets the role the contributor played.
    /// </summary>
    public MediaContributorRole Role { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicMediaContributor"/> class.
    /// </summary>
    /// <param name="contributorId">The unique identifier of the media contributor.</param>
    /// <param name="role">The role the contributor played.</param>
    private MusicMediaContributor(MediaContributorId contributorId, MediaContributorRole role)
    {
        ContributorId = contributorId;
        Role = role;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MusicMediaContributor"/> class.
    /// </summary>
    /// <param name="contributorId">The unique identifier of the media contributor.</param>
    /// <param name="role">The role the contributor played.</param>
    /// <returns>
    /// An <see cref="Result{TValue}"/> containing either a successfully created <see cref="MusicMediaContributor"/>, or an error message.
    /// </returns>
    public static Result<MusicMediaContributor> Create(MediaContributorId contributorId, MediaContributorRole role)
    {
        return new MusicMediaContributor(contributorId, role);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return ContributorId;
        yield return Role;
    }
}
