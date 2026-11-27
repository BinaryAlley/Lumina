#region ========================================================================= USING =====================================================================================
using Lumina.Application.Core.Themes.Management.Queries.GetThemeAsset;
using Lumina.Contracts.Requests.Themes;
#endregion

namespace Lumina.Application.Common.Mapping.Themes;

/// <summary>
/// Extension methods for converting <see cref="GetThemeAssetRequest"/>.
/// </summary>
public static class GetThemeAssetRequestMapping
{
    /// <summary>
    /// Converts <paramref name="request"/> to <see cref="GetThemeAssetQuery"/>.
    /// </summary>
    /// <param name="request">The request to be converted.</param>
    /// <returns>The converted query.</returns>
    public static GetThemeAssetQuery ToQuery(this GetThemeAssetRequest request)
    {
        return new GetThemeAssetQuery(request.ThemeId, request.AssetPath);
    }
}
