#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Fixture class for the <see cref="GetReadingSectionRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetReadingSectionRequestFixture
{
    /// <summary>
    /// Creates a random valid <see cref="GetReadingSectionRequest"/>.
    /// </summary>
    /// <param name="bookId">Optional. The Id of the book whose reading section is retrieved.</param>
    /// <param name="locationRef">Optional. The opaque location reference of the reading section.</param>
    /// <returns>The created <see cref="GetReadingSectionRequest"/>.</returns>
    public GetReadingSectionRequest Create(
        Guid? bookId = null,
        string? locationRef = null)
    {
        return new GetReadingSectionRequest(bookId ?? Guid.NewGuid(), locationRef ?? $"section-{Guid.NewGuid():N}");
    }

    /// <summary>
    /// Creates a list of <see cref="GetReadingSectionRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetReadingSectionRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
