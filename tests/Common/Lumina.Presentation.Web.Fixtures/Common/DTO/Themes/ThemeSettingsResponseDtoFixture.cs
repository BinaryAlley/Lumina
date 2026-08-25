#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;

/// <summary>
/// Fixture class for generating <see cref="ThemeSettingsResponseDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeSettingsResponseDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="ThemeSettingsResponseDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="maxArchiveBytes">Optional maximum allowed archive size, in bytes.</param>
    /// <param name="defaultThemeId">Optional identifier of the fallback theme.</param>
    /// <returns>A configured <see cref="ThemeSettingsResponseDto"/> instance.</returns>
    public ThemeSettingsResponseDto Create(
        long? maxArchiveBytes = null, 
        string? defaultThemeId = null)
    {
        return new ThemeSettingsResponseDto(
            MaxArchiveBytes: maxArchiveBytes ?? _faker.Random.Long(1024 * 1024, 256 * 1024 * 1024),
            DefaultThemeId: defaultThemeId ?? _faker.Lorem.Word()
        );
    }

    /// <summary>
    /// Creates multiple <see cref="ThemeSettingsResponseDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ThemeSettingsResponseDto"/> instances.</returns>
    public List<ThemeSettingsResponseDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
