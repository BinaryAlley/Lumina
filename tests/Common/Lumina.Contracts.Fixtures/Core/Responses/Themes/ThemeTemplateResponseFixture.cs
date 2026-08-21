#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.Themes;

/// <summary>
/// Fixture class for the <see cref="ThemeTemplateResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeTemplateResponseFixture
{
    private readonly Faker _faker = new();
    private readonly ThemeResponseFixture _themeResponseFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="ThemeTemplateResponse"/>.
    /// </summary>
    /// <param name="theme">Optional. The theme the template belongs to.</param>
    /// <param name="template">Optional. The sanitized content of the template.</param>
    /// <returns>The created <see cref="ThemeTemplateResponse"/>.</returns>
    public ThemeTemplateResponse Create(ThemeResponse? theme = null, string? template = null)
    {
        return new ThemeTemplateResponse(
            theme ?? _themeResponseFixture.Create(),
            template ?? _faker.Lorem.Paragraph());
    }

    /// <summary>
    /// Creates a list of <see cref="ThemeTemplateResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ThemeTemplateResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
