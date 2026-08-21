#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Themes;

/// <summary>
/// Fixture class for the <see cref="GetThemeArchiveRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetThemeArchiveRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="GetThemeArchiveRequest"/>.
    /// </summary>
    /// <param name="themeId">Optional. The manifest id of the theme.</param>
    /// <returns>The created <see cref="GetThemeArchiveRequest"/>.</returns>
    public GetThemeArchiveRequest Create(string? themeId = null)
    {
        return new GetThemeArchiveRequest(themeId ?? _faker.Lorem.Slug(2));
    }

    /// <summary>
    /// Creates a list of <see cref="GetThemeArchiveRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetThemeArchiveRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
