#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Reading;

/// <summary>
/// Fixture class for generating <see cref="ReadingManifestDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingManifestDtoFixture
{
    private readonly ReadingTocEntryDtoFixture _readingTocEntryDtoFixture = new();
    private readonly ReadingSpineItemDtoFixture _readingSpineItemDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="ReadingManifestDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="title">Optional title of the book.</param>
    /// <param name="author">Optional author of the book.</param>
    /// <param name="coverResourceKey">Optional resource key of the cover image of the book.</param>
    /// <param name="tableOfContents">Optional hierarchical table of contents of the book.</param>
    /// <param name="spine">Optional ordered spine of the reading sections of the book.</param>
    /// <param name="resourceKeys">Optional resource keys of the resources of the book.</param>
    /// <param name="hasTextContent">Optional value indicating whether the book has extractable text content.</param>
    /// <returns>A configured <see cref="ReadingManifestDto"/> instance.</returns>
    public ReadingManifestDto Create(
        string? title = null,
        string? author = null,
        string? coverResourceKey = null,
        List<ReadingTocEntryDto>? tableOfContents = null,
        List<ReadingSpineItemDto>? spine = null,
        List<string>? resourceKeys = null,
        bool? hasTextContent = null)
    {
        return new ReadingManifestDto
        {
            Title = title ?? $"Book {Guid.NewGuid():N}",
            Author = author,
            CoverResourceKey = coverResourceKey,
            TableOfContents = tableOfContents ?? [.. _readingTocEntryDtoFixture.CreateMany(2)],
            Spine = spine ?? [.. _readingSpineItemDtoFixture.CreateMany(2)],
            ResourceKeys = resourceKeys ?? [.. Enumerable.Range(0, 2).Select(_ => $"resource-{Guid.NewGuid():N}")],
            HasTextContent = hasTextContent ?? true
        };
    }

    /// <summary>
    /// Creates multiple <see cref="ReadingManifestDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ReadingManifestDto"/> instances.</returns>
    public List<ReadingManifestDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
