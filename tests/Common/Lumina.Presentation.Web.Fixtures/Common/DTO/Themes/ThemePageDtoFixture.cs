#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Themes;

/// <summary>
/// Fixture class for generating <see cref="ThemePageDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemePageDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="ThemePageDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="pageKey">Optional key of the page, used to select the template to render.</param>
    /// <param name="title">Optional title of the page.</param>
    /// <param name="description">Optional description of the page.</param>
    /// <param name="pageData">Optional resolved values exposed to the theme template.</param>
    /// <returns>A configured <see cref="ThemePageDto"/> instance.</returns>
    public ThemePageDto Create(
        string? pageKey = null,
        string? title = null,
        string? description = null,
        IReadOnlyDictionary<string, object?>? pageData = null)
    {
        return new ThemePageDto
        {
            PageKey = pageKey ?? _faker.Lorem.Word(),
            Title = title ?? _faker.Lorem.Sentence(),
            Description = description ?? _faker.Lorem.Sentence(),
            PageData = pageData ?? new Dictionary<string, object?>()
        };
    }

    /// <summary>
    /// Creates multiple <see cref="ThemePageDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ThemePageDto"/> instances.</returns>
    public List<ThemePageDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
