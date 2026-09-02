#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Reading;

/// <summary>
/// Fixture class for generating <see cref="ReadingTocEntryDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingTocEntryDtoFixture
{
    /// <summary>
    /// Creates a new <see cref="ReadingTocEntryDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="label">Optional label of the table of contents entry.</param>
    /// <param name="locationRef">Optional opaque location reference of the reading section the entry points to.</param>
    /// <param name="children">Optional child entries of the table of contents entry.</param>
    /// <returns>A configured <see cref="ReadingTocEntryDto"/> instance.</returns>
    public ReadingTocEntryDto Create(
        string? label = null,
        string? locationRef = null,
        List<ReadingTocEntryDto>? children = null)
    {
        return new ReadingTocEntryDto
        {
            Label = label ?? $"Chapter {Guid.NewGuid():N}",
            LocationRef = locationRef ?? $"section-{Guid.NewGuid():N}",
            Children = children ?? []
        };
    }

    /// <summary>
    /// Creates multiple <see cref="ReadingTocEntryDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ReadingTocEntryDto"/> instances.</returns>
    public List<ReadingTocEntryDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
