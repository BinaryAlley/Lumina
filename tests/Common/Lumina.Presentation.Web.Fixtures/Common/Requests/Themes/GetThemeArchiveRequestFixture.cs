#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Themes;

/// <summary>
/// Fixture class for generating <see cref="GetThemeArchiveRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeArchiveRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="GetThemeArchiveRequest"/> instance.
    /// </summary>
    /// <param name="themeId">Optional manifest id of the theme to download.</param>
    /// <returns>A configured <see cref="GetThemeArchiveRequest"/> instance.</returns>
    public GetThemeArchiveRequest Create(
        string? themeId = null)
    {
        return new GetThemeArchiveRequest(
            ThemeId: themeId
        );
    }

    /// <summary>
    /// Creates multiple <see cref="GetThemeArchiveRequest"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetThemeArchiveRequest"/> instances.</returns>
    public List<GetThemeArchiveRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
