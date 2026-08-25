#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;

/// <summary>
/// Fixture class for generating <see cref="ThemePageRenderResultDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemePageRenderResultDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="ThemePageRenderResultDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="content">Optional rendered HTML content of the page section.</param>
    /// <param name="script">Optional rendered script element of the page section.</param>
    /// <returns>A configured <see cref="ThemePageRenderResultDto"/> instance.</returns>
    public ThemePageRenderResultDto Create(
        string? content = null, 
        string? script = null)
    {
        return new ThemePageRenderResultDto(
            Content: content ?? _faker.Lorem.Paragraph(),
            Script: script ?? _faker.Lorem.Paragraph()
        );
    }

    /// <summary>
    /// Creates multiple <see cref="ThemePageRenderResultDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ThemePageRenderResultDto"/> instances.</returns>
    public List<ThemePageRenderResultDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
