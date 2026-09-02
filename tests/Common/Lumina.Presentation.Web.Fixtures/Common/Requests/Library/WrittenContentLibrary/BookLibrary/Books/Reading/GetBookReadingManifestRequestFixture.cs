#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Fixture class for the <see cref="GetBookReadingManifestRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetBookReadingManifestRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetBookReadingManifestRequest"/>.
    /// </summary>
    /// <param name="bookId">Optional. The Id of the book whose reading manifest is retrieved.</param>
    /// <returns>The created <see cref="GetBookReadingManifestRequest"/>.</returns>
    public GetBookReadingManifestRequest Create(
        Guid? bookId = null)
    {
        return new GetBookReadingManifestRequest(bookId ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="GetBookReadingManifestRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetBookReadingManifestRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
