#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;

/// <summary>
/// Fixture class for generating <see cref="ThemeTemplateResponseDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeTemplateResponseDtoFixture
{
    private readonly Faker _faker = new();
    private readonly ThemeResponseDtoFixture _themeResponseDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="ThemeTemplateResponseDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="theme">Optional theme the template belongs to.</param>
    /// <param name="template">Optional sanitized template content.</param>
    /// <returns>A configured <see cref="ThemeTemplateResponseDto"/> instance.</returns>
    public ThemeTemplateResponseDto Create(
        ThemeResponseDto? theme = null, 
        string? template = null)
    {
        return new ThemeTemplateResponseDto(
            Theme: theme ?? _themeResponseDtoFixture.Create(),
            Template: template ?? _faker.Lorem.Paragraph()
        );
    }

    /// <summary>
    /// Creates multiple <see cref="ThemeTemplateResponseDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ThemeTemplateResponseDto"/> instances.</returns>
    public List<ThemeTemplateResponseDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
