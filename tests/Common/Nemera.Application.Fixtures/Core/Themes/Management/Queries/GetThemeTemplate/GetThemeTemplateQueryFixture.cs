#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Themes.Management.Queries.GetThemeTemplate;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Themes.Management.Queries.GetThemeTemplate;

/// <summary>
/// Fixture class for the <see cref="GetThemeTemplateQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeTemplateQueryFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="GetThemeTemplateQuery"/>.
    /// </summary>
    /// <param name="themeId">Optional manifest id of the theme.</param>
    /// <param name="pageKey">Optional page key that selects the template.</param>
    /// <returns>The created <see cref="GetThemeTemplateQuery"/>.</returns>
    public GetThemeTemplateQuery Create(string? themeId = null, string? pageKey = null)
    {
        return new GetThemeTemplateQuery(
            themeId ?? _faker.Lorem.Slug(2),
            pageKey ?? _faker.Lorem.Word());
    }

    /// <summary>
    /// Creates a list of <see cref="GetThemeTemplateQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetThemeTemplateQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
