#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Themes;

/// <summary>
/// Fixture class for the <see cref="ThemeSettingsResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeSettingsResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ThemeSettingsResponse"/>.
    /// </summary>
    /// <param name="maxArchiveBytes">Optional. The maximum allowed size of a theme archive, in bytes.</param>
    /// <param name="allowThemeScripts">Optional. Whether theme templates may contain script elements.</param>
    /// <param name="defaultThemeId">Optional. The identifier of the default theme.</param>
    /// <returns>The created <see cref="ThemeSettingsResponse"/>.</returns>
    public ThemeSettingsResponse Create(
        long? maxArchiveBytes = null,
        bool? allowThemeScripts = null,
        string? defaultThemeId = null)
    {
        return new ThemeSettingsResponse(
            maxArchiveBytes ?? _faker.Random.Long(1_000_000, 100_000_000),
            allowThemeScripts ?? _faker.Random.Bool(),
            defaultThemeId ?? _faker.Lorem.Slug(2));
    }

    /// <summary>
    /// Creates a list of <see cref="ThemeSettingsResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ThemeSettingsResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
