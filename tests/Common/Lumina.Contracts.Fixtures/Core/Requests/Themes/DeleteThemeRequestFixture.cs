#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.Themes;

/// <summary>
/// Fixture class for the <see cref="DeleteThemeRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeleteThemeRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="DeleteThemeRequest"/>.
    /// </summary>
    /// <param name="themeId">Optional. The manifest id of the theme to delete.</param>
    /// <returns>The created <see cref="DeleteThemeRequest"/>.</returns>
    public DeleteThemeRequest Create(string? themeId = null)
    {
        return new DeleteThemeRequest(themeId ?? _faker.Lorem.Slug(2));
    }

    /// <summary>
    /// Creates a list of <see cref="DeleteThemeRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<DeleteThemeRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
