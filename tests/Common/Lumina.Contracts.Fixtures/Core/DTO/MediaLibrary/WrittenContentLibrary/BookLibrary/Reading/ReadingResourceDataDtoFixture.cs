#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Fixture class for the <see cref="ReadingResourceDataDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingResourceDataDtoFixture
{
    /// <summary>
    /// Creates a random valid <see cref="ReadingResourceDataDto"/>.
    /// </summary>
    /// <param name="data">Optional. The binary data of the resource.</param>
    /// <param name="mimeType">Optional. The MIME type of the resource.</param>
    /// <returns>The created <see cref="ReadingResourceDataDto"/>.</returns>
    public ReadingResourceDataDto Create(
        byte[]? data = null,
        string? mimeType = null)
    {
        return new ReadingResourceDataDto(data ?? Guid.NewGuid().ToByteArray(), mimeType ?? "image/png");
    }

    /// <summary>
    /// Creates a list of <see cref="ReadingResourceDataDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ReadingResourceDataDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
