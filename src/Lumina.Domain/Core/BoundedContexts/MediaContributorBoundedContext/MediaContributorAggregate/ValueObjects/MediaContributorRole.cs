#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Errors;
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.MediaContributors;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MediaContributorBoundedContext.MediaContributorAggregate.ValueObjects;

/// <summary>
/// Value Object for the role of a media contributor in a media item.
/// The role carries both a free-form display name, as returned by the metadata providers, and the canonical
/// category the display name normalizes to, so that roles describing the same kind of contribution are never
/// treated as distinct.
/// </summary>
[DebuggerDisplay("{DisplayName}")]
public sealed class MediaContributorRole : ValueObject
{
    /// <summary>
    /// Gets the display name of the role.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the canonical category of the role.
    /// </summary>
    public MediaContributorRoleCategory Category { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaContributorRole"/> class.
    /// </summary>
    /// <param name="displayName">The display name of the role.</param>
    /// <param name="category">The canonical category of the role.</param>
    private MediaContributorRole(string displayName, MediaContributorRoleCategory category)
    {
        DisplayName = displayName;
        Category = category;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaContributorRole"/> class, from a display <paramref name="displayName"/> and its canonical <paramref name="category"/>.
    /// </summary>
    /// <param name="displayName">The display name of the role.</param>
    /// <param name="category">The canonical category of the role.</param>
    /// <returns>The created <see cref="MediaContributorRole"/> instance.</returns>
    public static Result<MediaContributorRole> Create(string displayName, MediaContributorRoleCategory category)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return Errors.MediaContributors.MediaContributorRoleNameCannotBeEmpty;

        return new MediaContributorRole(displayName, category);
    }

    /// <summary>
    /// Gets the list of items that define equality of the object.
    /// </summary>
    /// <returns>A list of items defining the equality.</returns>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return DisplayName;
        yield return Category;
    }
}
