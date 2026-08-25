#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Themes;

/// <summary>
/// Fixture class for generating <see cref="RestoreThemeRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class RestoreThemeRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="RestoreThemeRequest"/> instance.
    /// </summary>
    /// <param name="themeId">Optional manifest id of the bundled theme to restore.</param>
    /// <returns>A configured <see cref="RestoreThemeRequest"/> instance.</returns>
    public RestoreThemeRequest Create(
        string? themeId = null)
    {
        return new RestoreThemeRequest(
            ThemeId: themeId
        );
    }

    /// <summary>
    /// Creates multiple <see cref="RestoreThemeRequest"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="RestoreThemeRequest"/> instances.</returns>
    public List<RestoreThemeRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
