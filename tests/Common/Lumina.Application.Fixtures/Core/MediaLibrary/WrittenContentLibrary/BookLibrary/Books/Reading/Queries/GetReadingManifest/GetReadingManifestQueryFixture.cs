#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingManifest;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading.Queries.GetReadingManifest;

/// <summary>
/// Fixture class for the <see cref="GetReadingManifestQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingManifestQueryFixture
{
    /// <summary>
    /// Creates a random valid query to get the reading manifest of a book.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the library the book belongs to.</param>
    /// <param name="bookId">Optional. The Id of the book whose reading manifest is retrieved.</param>
    /// <returns>The created <see cref="GetReadingManifestQuery"/>.</returns>
    public GetReadingManifestQuery Create(
        string? libraryId = null,
        string? bookId = null)
    {
        return new GetReadingManifestQuery(libraryId ?? Guid.NewGuid().ToString(), bookId ?? Guid.NewGuid().ToString());
    }

    /// <summary>
    /// Creates a list of <see cref="GetReadingManifestQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetReadingManifestQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
