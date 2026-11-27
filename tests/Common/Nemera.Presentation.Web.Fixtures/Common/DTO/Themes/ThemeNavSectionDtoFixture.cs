#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;

/// <summary>
/// Fixture class for the <see cref="ThemeNavSectionDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeNavSectionDtoFixture
{
    private readonly Faker _faker = new();
    private readonly ThemeNavEntryDtoFixture _themeNavEntryDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="ThemeNavSectionDto"/>.
    /// </summary>
    /// <param name="label">Optional label of the section.</param>
    /// <param name="items">Optional entries of the section.</param>
    /// <returns>The created <see cref="ThemeNavSectionDto"/>.</returns>
    public ThemeNavSectionDto Create(
        string? label = null,
        IReadOnlyList<ThemeNavEntryDto>? items = null)
    {
        return new ThemeNavSectionDto(
            Label: label ?? _faker.Lorem.Word(),
            Items: items ?? [_themeNavEntryDtoFixture.Create()]);
    }

    /// <summary>
    /// Creates a list of <see cref="ThemeNavSectionDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ThemeNavSectionDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
