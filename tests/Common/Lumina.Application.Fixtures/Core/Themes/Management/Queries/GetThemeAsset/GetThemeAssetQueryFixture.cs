#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Themes.Management.Queries.GetThemeAsset;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Themes.Management.Queries.GetThemeAsset;

/// <summary>
/// Fixture class for the <see cref="GetThemeAssetQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeAssetQueryFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="GetThemeAssetQuery"/>.
    /// </summary>
    /// <param name="themeId">Optional manifest id of the theme.</param>
    /// <param name="assetPath">Optional asset path relative to the theme pack root.</param>
    /// <returns>The created <see cref="GetThemeAssetQuery"/>.</returns>
    public GetThemeAssetQuery Create(string? themeId = null, string? assetPath = null)
    {
        return new GetThemeAssetQuery(
            themeId ?? _faker.Lorem.Slug(2),
            assetPath ?? _faker.System.FilePath());
    }

    /// <summary>
    /// Creates a list of <see cref="GetThemeAssetQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetThemeAssetQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
