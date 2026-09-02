#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Plugins.Queries.GetLibraryBookReaders;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Plugins.Queries.GetLibraryBookReaders;

/// <summary>
/// Fixture class for the <see cref="GetLibraryBookReadersQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetLibraryBookReadersQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get the book readers configured for a media library.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library whose book readers are retrieved.</param>
    /// <returns>The created <see cref="GetLibraryBookReadersQuery"/>.</returns>
    public GetLibraryBookReadersQuery Create(
        Guid? libraryId = null)
    {
        return new GetLibraryBookReadersQuery(libraryId ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="GetLibraryBookReadersQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetLibraryBookReadersQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
