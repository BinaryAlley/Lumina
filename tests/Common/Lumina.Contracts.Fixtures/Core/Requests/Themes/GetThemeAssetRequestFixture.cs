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
    /// <returns>The created <see cref="GetThemeAssetRequest"/>.</returns>
    public GetThemeAssetRequest Create(string? themeId = null, string? assetPath = null)
    {
        return new GetThemeAssetRequest(
            themeId ?? _faker.Lorem.Slug(2),
            assetPath ?? _faker.System.FilePath());
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
