#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Themes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;

/// <summary>
/// Fixture class for generating <see cref="ThemeNavMenuDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeNavMenuDtoFixture
{
    private readonly Faker _faker = new();
    private readonly ThemeNavSectionDtoFixture _themeNavSectionDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="ThemeNavMenuDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="siteName">Optional name of the site displayed by the navigation menu.</param>
    /// <param name="mobileSections">Optional sections of the mobile navigation menu.</param>
    /// <param name="menubarSections">Optional sections of the desktop navigation menu bar.</param>
    /// <returns>A configured <see cref="ThemeNavMenuDto"/> instance.</returns>
    public ThemeNavMenuDto Create(
        string? siteName = null,
        IReadOnlyList<ThemeNavSectionDto>? mobileSections = null,
        IReadOnlyList<ThemeNavSectionDto>? menubarSections = null)
    {
        ThemeNavSectionDto[] generatedSections = [_themeNavSectionDtoFixture.Create()];
        return new ThemeNavMenuDto(
            SiteName: siteName ?? _faker.Company.CompanyName(),
            MobileSections: mobileSections ?? generatedSections,
            MenubarSections: menubarSections ?? generatedSections
        );
    }

    /// <summary>
    /// Creates multiple <see cref="ThemeNavMenuDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ThemeNavMenuDto"/> instances.</returns>
    public List<ThemeNavMenuDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
