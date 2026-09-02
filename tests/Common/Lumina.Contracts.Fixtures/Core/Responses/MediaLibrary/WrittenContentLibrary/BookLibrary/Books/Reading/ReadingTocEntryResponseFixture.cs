#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.WrittenContentLibrary.BookLibrary.Books.Reading;

/// <summary>
/// Fixture class for the <see cref="ReadingTocEntryResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingTocEntryResponseFixture
{
    /// <summary>
    /// Creates a random valid <see cref="ReadingTocEntryResponse"/>.
    /// </summary>
    /// <param name="label">Optional. The label of the table of contents entry.</param>
    /// <param name="locationRef">Optional. The opaque location reference of the reading section the entry points to.</param>
    /// <param name="children">Optional. The child entries of the table of contents entry.</param>
    /// <returns>The created <see cref="ReadingTocEntryResponse"/>.</returns>
    public ReadingTocEntryResponse Create(
        string? label = null,
        string? locationRef = null,
        IReadOnlyList<ReadingTocEntryResponse>? children = null)
    {
        return new ReadingTocEntryResponse(
            label ?? $"Chapter {Guid.NewGuid():N}",
            locationRef ?? $"section-{Guid.NewGuid():N}",
            children ?? []
        );
    }

    /// <summary>
    /// Creates a list of <see cref="ReadingTocEntryResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ReadingTocEntryResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
