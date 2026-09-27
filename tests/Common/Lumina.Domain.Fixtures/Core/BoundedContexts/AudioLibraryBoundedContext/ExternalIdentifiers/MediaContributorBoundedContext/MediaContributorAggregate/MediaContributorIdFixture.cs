#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.MediaContributorBoundedContext.MediaContributorAggregate;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.ExternalIdentifiers.MediaContributorBoundedContext.MediaContributorAggregate;

/// <summary>
/// Fixture class for the <see cref="MediaContributorId"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaContributorIdFixture
{
    /// <summary>
    /// Creates a random valid <see cref="MediaContributorId"/>.
    /// </summary>
    /// <param name="value">Optional. The raw value of the media contributor Id.</param>
    /// <returns>The created <see cref="MediaContributorId"/>.</returns>
    public MediaContributorId Create(
        Guid? value = null)
    {
        return value is null ? MediaContributorId.CreateUnique() : MediaContributorId.Create(value.Value);
    }

    /// <summary>
    /// Creates a list of <see cref="MediaContributorId"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaContributorId"/> instances.</returns>
    public List<MediaContributorId> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
