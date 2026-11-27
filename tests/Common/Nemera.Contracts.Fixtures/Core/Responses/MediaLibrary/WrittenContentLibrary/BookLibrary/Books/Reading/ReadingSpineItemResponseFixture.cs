#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Fixture class for the <see cref="ReadingSpineItemResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingSpineItemResponseFixture
{
    /// <summary>
    /// Creates a random valid <see cref="ReadingSpineItemResponse"/>.
    /// </summary>
    /// <param name="locationRef">Optional. The opaque location reference of the reading section.</param>
    /// <param name="title">Optional. The title of the reading section, if known.</param>
    /// <returns>The created <see cref="ReadingSpineItemResponse"/>.</returns>
    public ReadingSpineItemResponse Create(
        string? locationRef = null,
        string? title = null)
    {
        return new ReadingSpineItemResponse(locationRef ?? $"section-{Guid.NewGuid():N}", title);
    }

    /// <summary>
    /// Creates a list of <see cref="ReadingSpineItemResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ReadingSpineItemResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
