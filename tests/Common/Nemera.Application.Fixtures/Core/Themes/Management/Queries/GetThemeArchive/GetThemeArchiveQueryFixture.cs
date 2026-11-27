#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.Themes.Management.Queries.GetThemeArchive;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.Themes.Management.Queries.GetThemeArchive;

/// <summary>
/// Fixture class for the <see cref="GetThemeArchiveQuery"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeArchiveQueryFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="GetThemeArchiveQuery"/>.
    /// </summary>
    /// <param name="themeId">Optional manifest id of the theme.</param>
    /// <returns>The created <see cref="GetThemeArchiveQuery"/>.</returns>
    public GetThemeArchiveQuery Create(string? themeId = null)
    {
        return new GetThemeArchiveQuery(themeId ?? _faker.Lorem.Slug(2));
    }

    /// <summary>
    /// Creates a list of <see cref="GetThemeArchiveQuery"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetThemeArchiveQuery> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
