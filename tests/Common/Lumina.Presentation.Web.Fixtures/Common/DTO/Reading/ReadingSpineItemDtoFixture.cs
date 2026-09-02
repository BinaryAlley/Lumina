#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Reading;

/// <summary>
/// Fixture class for generating <see cref="ReadingSpineItemDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingSpineItemDtoFixture
{
    /// <summary>
    /// Creates a new <see cref="ReadingSpineItemDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="locationRef">Optional opaque location reference of the reading section.</param>
    /// <param name="title">Optional title of the reading section.</param>
    /// <returns>A configured <see cref="ReadingSpineItemDto"/> instance.</returns>
    public ReadingSpineItemDto Create(
        string? locationRef = null,
        string? title = null)
    {
        return new ReadingSpineItemDto
        {
            LocationRef = locationRef ?? $"section-{Guid.NewGuid():N}",
            Title = title
        };
    }

    /// <summary>
    /// Creates multiple <see cref="ReadingSpineItemDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ReadingSpineItemDto"/> instances.</returns>
    public List<ReadingSpineItemDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
