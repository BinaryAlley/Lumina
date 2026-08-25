#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;

/// <summary>
/// Fixture class for the <see cref="ThemeLayoutPageDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeLayoutPageDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ThemeLayoutPageDto"/>.
    /// </summary>
    /// <param name="title">Optional. The title of the page, displayed by the themed shell.</param>
    /// <param name="content">Optional. The rendered HTML content of the page section.</param>
    /// <param name="script">Optional. The rendered script element of the page section, when the template defines one.</param>
    /// <returns>The created <see cref="ThemeLayoutPageDto"/>.</returns>
    public ThemeLayoutPageDto Create(
        string? title = null, 
        string? content = null, 
        string? script = null)
    {
        return new ThemeLayoutPageDto(
            title ?? _faker.Lorem.Word(),
            content ?? _faker.Lorem.Paragraph(),
            script ?? _faker.Lorem.Word());
    }

    /// <summary>
    /// Creates a list of <see cref="ThemeLayoutPageDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ThemeLayoutPageDto"/> instances.</returns>
    public List<ThemeLayoutPageDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
