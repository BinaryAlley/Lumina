#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;

/// <summary>
/// Fixture class for the <see cref="ThemeNavEntryDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeNavEntryDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ThemeNavEntryDto"/>.
    /// </summary>
    /// <param name="label">Optional label of the entry.</param>
    /// <param name="url">Optional URL of the link, or <see langword="null"/> for a submenu.</param>
    /// <param name="includeUrl">Whether to set <paramref name="url"/>, or <see langword="null"/> when <see langword="false"/>.</param>
    /// <param name="cssClass">Optional CSS classes of the link.</param>
    /// <param name="includeCssClass">Whether to set <paramref name="cssClass"/>, or <see langword="null"/> when <see langword="false"/>.</param>
    /// <param name="children">Optional child links of a submenu.</param>
    /// <returns>The created <see cref="ThemeNavEntryDto"/>.</returns>
    public ThemeNavEntryDto Create(
        string? label = null,
        string? url = null,
        bool includeUrl = false,
        string? cssClass = null,
        bool includeCssClass = false,
        IReadOnlyList<ThemeNavEntryDto>? children = null)
    {
        return new ThemeNavEntryDto(
            Label: label ?? _faker.Lorem.Word(),
            Url: includeUrl ? (url ?? _faker.Internet.Url()) : null,
            CssClass: includeCssClass ? (cssClass ?? _faker.Lorem.Word()) : null,
            Children: children ?? []);
    }

    /// <summary>
    /// Creates a list of <see cref="ThemeNavEntryDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ThemeNavEntryDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
