#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Fixture class for the <see cref="ReadingSpineItemDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingSpineItemDtoFixture
{
    /// <summary>
    /// Creates a random valid <see cref="ReadingSpineItemDto"/>.
    /// </summary>
    /// <param name="locationRef">Optional. The opaque location reference of the reading section.</param>
    /// <param name="title">Optional. The title of the reading section, if known.</param>
    /// <param name="relativeSectionFilePath">Optional. The path of the section content file, relative to the extraction directory of the book.</param>
    /// <returns>The created <see cref="ReadingSpineItemDto"/>.</returns>
    public ReadingSpineItemDto Create(
        string? locationRef = null,
        string? title = null,
        string? relativeSectionFilePath = null)
    {
        return new ReadingSpineItemDto(locationRef ?? $"section-{Guid.NewGuid():N}", title, relativeSectionFilePath ?? $"sections/{Guid.NewGuid():N}.html");
    }

    /// <summary>
    /// Creates a list of <see cref="ReadingSpineItemDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ReadingSpineItemDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
