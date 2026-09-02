#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Fixture class for the <see cref="GetReadingManifestRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingManifestRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetReadingManifestRequest"/>.
    /// </summary>
    /// <param name="bookId">Optional. The Id of the book whose reading manifest is retrieved.</param>
    /// <returns>The created <see cref="GetReadingManifestRequest"/>.</returns>
    public GetReadingManifestRequest Create(
        Guid? bookId = null)
    {
        return new GetReadingManifestRequest(bookId ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="GetReadingManifestRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetReadingManifestRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
