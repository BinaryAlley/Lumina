#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Themes;

/// <summary>
/// Fixture class for generating <see cref="SetCurrentThemeRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class SetCurrentThemeRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="SetCurrentThemeRequest"/> instance.
    /// </summary>
    /// <param name="themeId">Optional manifest id of the theme to activate.</param>
    /// <returns>A configured <see cref="SetCurrentThemeRequest"/> instance.</returns>
    public SetCurrentThemeRequest Create(
        string? themeId = null)
    {
        return new SetCurrentThemeRequest(
            ThemeId: themeId
        );
    }

    /// <summary>
    /// Creates multiple <see cref="SetCurrentThemeRequest"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="SetCurrentThemeRequest"/> instances.</returns>
    public List<SetCurrentThemeRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
