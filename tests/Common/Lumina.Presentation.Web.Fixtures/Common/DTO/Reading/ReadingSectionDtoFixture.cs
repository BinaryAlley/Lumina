#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.DTO.Reading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Reading;

/// <summary>
/// Fixture class for generating <see cref="ReadingSectionDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ReadingSectionDtoFixture
{
    /// <summary>
    /// Creates a new <see cref="ReadingSectionDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="locationRef">Optional opaque location reference of the reading section.</param>
    /// <param name="title">Optional title of the reading section.</param>
    /// <param name="contentHtml">Optional sanitized HTML content of the reading section.</param>
    /// <returns>A configured <see cref="ReadingSectionDto"/> instance.</returns>
    public ReadingSectionDto Create(
        string? locationRef = null,
        string? title = null,
        string? contentHtml = null)
    {
        return new ReadingSectionDto
        {
            LocationRef = locationRef ?? $"section-{Guid.NewGuid():N}",
            Title = title,
            ContentHtml = contentHtml ?? "<section><p>Content</p></section>"
        };
    }

    /// <summary>
    /// Creates multiple <see cref="ReadingSectionDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ReadingSectionDto"/> instances.</returns>
    public List<ReadingSectionDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
