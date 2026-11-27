#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Themes;

/// <summary>
/// Fixture class for the <see cref="GetThemeAssetRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeAssetRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="GetThemeAssetRequest"/>.
    /// </summary>
    /// <param name="themeId">Optional. The manifest id of the theme.</param>
    /// <param name="assetPath">Optional. The asset path relative to the theme pack root.</param>
    /// <param name="includeThemeId">Whether the theme Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAssetPath">Whether the asset path should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="GetThemeAssetRequest"/>.</returns>
    public GetThemeAssetRequest Create(
        string? themeId = null,
        string? assetPath = null,
        bool includeThemeId = true,
        bool includeAssetPath = true)
    {
        return new GetThemeAssetRequest(
            includeThemeId ? (themeId ?? _faker.Lorem.Slug(2)) : null,
            includeAssetPath ? (assetPath ?? _faker.System.FilePath()) : null
        );
    }

    /// <summary>
    /// Creates a list of <see cref="GetThemeAssetRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetThemeAssetRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
