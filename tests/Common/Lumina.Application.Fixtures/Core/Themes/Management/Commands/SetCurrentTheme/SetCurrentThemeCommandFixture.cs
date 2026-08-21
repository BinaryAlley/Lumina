#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Themes.Management.Commands.SetCurrentTheme;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Themes.Management.Commands.SetCurrentTheme;

/// <summary>
/// Fixture class for the <see cref="SetCurrentThemeCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetCurrentThemeCommandFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="SetCurrentThemeCommand"/>.
    /// </summary>
    /// <param name="themeId">Optional manifest id of the theme to activate.</param>
    /// <returns>The created <see cref="SetCurrentThemeCommand"/>.</returns>
    public SetCurrentThemeCommand Create(string? themeId = null)
    {
        return new SetCurrentThemeCommand(themeId ?? _faker.Lorem.Slug(2));
    }

    /// <summary>
    /// Creates a list of <see cref="SetCurrentThemeCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<SetCurrentThemeCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
