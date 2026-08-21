#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Themes;

/// <summary>
/// Fixture class for the <see cref="GetThemeTemplateRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeTemplateRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="GetThemeTemplateRequest"/>.
    /// </summary>
    /// <param name="themeId">Optional. The manifest id of the theme.</param>
    /// <param name="pageKey">Optional. The page key that selects the template.</param>
    /// <returns>The created <see cref="GetThemeTemplateRequest"/>.</returns>
    public GetThemeTemplateRequest Create(string? themeId = null, string? pageKey = null)
    {
        return new GetThemeTemplateRequest(
            themeId ?? _faker.Lorem.Slug(2),
            pageKey ?? _faker.Lorem.Word());
    }

    /// <summary>
    /// Creates a list of <see cref="GetThemeTemplateRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetThemeTemplateRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
