#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Themes;

/// <summary>
/// Fixture class for generating <see cref="DeleteThemeRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteThemeRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="DeleteThemeRequest"/> instance.
    /// </summary>
    /// <param name="themeId">Optional manifest id of the theme to delete.</param>
    /// <returns>A configured <see cref="DeleteThemeRequest"/> instance.</returns>
    public DeleteThemeRequest Create(
        string? themeId = null)
    {
        return new DeleteThemeRequest(
            ThemeId: themeId
        );
    }

    /// <summary>
    /// Creates multiple <see cref="DeleteThemeRequest"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DeleteThemeRequest"/> instances.</returns>
    public List<DeleteThemeRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
