#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.WrittenContentLibrary.BookLibrary.Reading;

/// <summary>
/// Fixture class for the <see cref="ReadingSectionDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingSectionDtoFixture
{
    /// <summary>
    /// Creates a random valid <see cref="ReadingSectionDto"/>.
    /// </summary>
    /// <param name="locationRef">Optional. The opaque location reference of the reading section.</param>
    /// <param name="title">Optional. The title of the reading section, if known.</param>
    /// <param name="contentHtml">Optional. The sanitized HTML content of the reading section.</param>
    /// <returns>The created <see cref="ReadingSectionDto"/>.</returns>
    public ReadingSectionDto Create(
        string? locationRef = null,
        string? title = null,
        string? contentHtml = null)
    {
        return new ReadingSectionDto(locationRef ?? $"section-{Guid.NewGuid():N}", title, contentHtml ?? "<section><p>Content</p></section>");
    }

    /// <summary>
    /// Creates a list of <see cref="ReadingSectionDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ReadingSectionDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
