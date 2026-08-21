#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Themes.Management.Commands.DeleteTheme;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Themes.Management.Commands.DeleteTheme;

/// <summary>
/// Fixture class for the <see cref="DeleteThemeCommand"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteThemeCommandFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="DeleteThemeCommand"/>.
    /// </summary>
    /// <param name="themeId">Optional manifest id of the theme to delete.</param>
    /// <returns>The created <see cref="DeleteThemeCommand"/>.</returns>
    public DeleteThemeCommand Create(string? themeId = null)
    {
        return new DeleteThemeCommand(themeId ?? _faker.Lorem.Slug(2));
    }

    /// <summary>
    /// Creates a list of <see cref="DeleteThemeCommand"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<DeleteThemeCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
