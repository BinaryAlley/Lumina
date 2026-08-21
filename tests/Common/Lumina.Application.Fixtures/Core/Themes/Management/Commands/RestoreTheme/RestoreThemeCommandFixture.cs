#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Themes.Management.Commands.RestoreTheme;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Themes.Management.Commands.RestoreTheme;

/// <summary>
/// Fixture class for the <see cref="RestoreThemeCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreThemeCommandFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="RestoreThemeCommand"/>.
    /// </summary>
    /// <param name="themeId">Optional manifest id of the theme to restore.</param>
    /// <returns>The created <see cref="RestoreThemeCommand"/>.</returns>
    public RestoreThemeCommand Create(string? themeId = null)
    {
        return new RestoreThemeCommand(themeId ?? _faker.Lorem.Slug(2));
    }

    /// <summary>
    /// Creates a list of <see cref="RestoreThemeCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<RestoreThemeCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
