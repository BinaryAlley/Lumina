#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.Aggregates.MediaContributor.MediaContributorAggregate.ValueObjects;

/// <summary>
/// Value Object for the role of a media contributor.
/// </summary>
[DebuggerDisplay("{DisplayName}")]
public sealed class MediaContributorRole : ValueObject
{
    /// <summary>
    /// Gets the value representing this object.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the category of this object.
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaContributorRole"/> class.
    /// </summary>
    /// <param name="name">The value representing this object.</param>
    /// <param name="category">The category of this object.</param>
    private MediaContributorRole(string name, string category)
    {
        Name = name;
        Category = category;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaContributorRole"/> class, from a pre-existing <paramref name="value"/>.
    /// </summary>
    /// <param name="name">The value representing this object.</param>
    /// <param name="category">The category of this object.</param>
    /// <returns>The created <see cref="MediaContributorRole"/> instance.</returns>
    public static MediaContributorRole Create(string name, string category)
    {
        // TODO: enforce invariants
        return new MediaContributorRole(name, category);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return Category;
    }
}