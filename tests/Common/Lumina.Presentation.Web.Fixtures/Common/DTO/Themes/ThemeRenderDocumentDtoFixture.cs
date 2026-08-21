#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;

/// <summary>
/// Fixture class for generating <see cref="ThemeRenderDocumentDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeRenderDocumentDtoFixture
{
    private readonly ThemeInfoDtoFixture _themeInfoDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="ThemeRenderDocumentDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="theme">Optional metadata of the resolved theme.</param>
    /// <param name="template">Optional raw template source to render.</param>
    /// <returns>A configured <see cref="ThemeRenderDocumentDto"/> instance.</returns>
    public ThemeRenderDocumentDto Create(ThemeInfoDto? theme = null, string? template = null)
    {
        Faker faker = new();
        return new ThemeRenderDocumentDto(
            Theme: theme ?? _themeInfoDtoFixture.Create(),
            Template: template ?? faker.Lorem.Paragraph()
        );
    }

    /// <summary>
    /// Creates multiple <see cref="ThemeRenderDocumentDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ThemeRenderDocumentDto"/> instances.</returns>
    public List<ThemeRenderDocumentDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
