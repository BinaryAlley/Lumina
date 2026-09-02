#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Fixture class for the <see cref="ReadingResourceInfoDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingResourceInfoDtoFixture
{
    /// <summary>
    /// Creates a random valid <see cref="ReadingResourceInfoDto"/>.
    /// </summary>
    /// <param name="relativeFilePath">Optional. The path of the resource file, relative to the extraction directory of the book.</param>
    /// <param name="mimeType">Optional. The MIME type of the resource.</param>
    /// <returns>The created <see cref="ReadingResourceInfoDto"/>.</returns>
    public ReadingResourceInfoDto Create(
        string? relativeFilePath = null,
        string? mimeType = null)
    {
        return new ReadingResourceInfoDto(relativeFilePath ?? $"resources/resource-{Guid.NewGuid():N}.png", mimeType ?? "image/png");
    }

    /// <summary>
    /// Creates a list of <see cref="ReadingResourceInfoDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ReadingResourceInfoDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
