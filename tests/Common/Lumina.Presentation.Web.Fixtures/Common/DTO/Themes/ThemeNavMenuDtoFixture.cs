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
        Faker faker = new();
        ThemeNavSectionDto[] generatedSections = [CreateSection()];
        return new ThemeNavMenuDto(
            SiteName: siteName ?? faker.Company.CompanyName(),
            MobileSections: mobileSections ?? generatedSections,
            MenubarSections: menubarSections ?? generatedSections
        );
    }

    /// <summary>
    /// Creates a single <see cref="ThemeNavSectionDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="label">Optional label of the section.</param>
    /// <param name="items">Optional entries of the section.</param>
    /// <returns>A configured <see cref="ThemeNavSectionDto"/> instance.</returns>
    public ThemeNavSectionDto CreateSection(string? label = null, IReadOnlyList<ThemeNavEntryDto>? items = null)
    {
        Faker faker = new();
        return new ThemeNavSectionDto(
            Label: label ?? faker.Lorem.Word(),
            Items: items ?? [CreateEntry()]
        );
    }

    /// <summary>
    /// Creates a single <see cref="ThemeNavEntryDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="label">Optional label of the entry.</param>
    /// <param name="url">Optional URL of the link, or <see langword="null"/> for a submenu.</param>
    /// <param name="includeUrl">Whether to set <paramref name="url"/>, or <see langword="null"/> when <see langword="false"/>.</param>
    /// <param name="cssClass">Optional CSS classes of the link.</param>
    /// <param name="includeCssClass">Whether to set <paramref name="cssClass"/>, or <see langword="null"/> when <see langword="false"/>.</param>
    /// <param name="children">Optional child links of a submenu.</param>
    /// <returns>A configured <see cref="ThemeNavEntryDto"/> instance.</returns>
    public ThemeNavEntryDto CreateEntry(
        string? label = null,
        string? url = null,
        bool includeUrl = false,
        string? cssClass = null,
        bool includeCssClass = false,
        IReadOnlyList<ThemeNavEntryDto>? children = null)
    {
        Faker faker = new();
        return new ThemeNavEntryDto(
            Label: label ?? faker.Lorem.Word(),
            Url: includeUrl ? (url ?? faker.Internet.Url()) : null,
            CssClass: includeCssClass ? (cssClass ?? faker.Lorem.Word()) : null,
            Children: children ?? []
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
