#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Themes;

/// <summary>
/// Fixture class for generating <see cref="GetThemeAssetRequest"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeAssetRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="GetThemeAssetRequest"/> instance.
    /// </summary>
    /// <param name="themeId">Optional manifest id of the theme.</param>
    /// <param name="path">Optional asset path relative to the theme pack root.</param>
    /// <returns>A configured <see cref="GetThemeAssetRequest"/> instance.</returns>
    public GetThemeAssetRequest Create(
        string? themeId = null, 
        string? path = null)
    {
        return new GetThemeAssetRequest(
            ThemeId: themeId,
            Path: path
        );
    }

    /// <summary>
    /// Creates multiple <see cref="GetThemeAssetRequest"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetThemeAssetRequest"/> instances.</returns>
    public List<GetThemeAssetRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
