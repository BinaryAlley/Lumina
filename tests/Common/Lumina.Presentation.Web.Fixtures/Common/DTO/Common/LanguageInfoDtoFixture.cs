#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Common;

/// <summary>
/// Fixture class for generating <see cref="LanguageInfoDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class LanguageInfoDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="LanguageInfoDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="languageCode">Optional. The ISO 639-1 two-letter language code.</param>
    /// <param name="languageName">Optional. The name of the language in English.</param>
    /// <param name="nativeName">Optional. The native name of the language.</param>
    /// <returns>A configured <see cref="LanguageInfoDto"/> instance.</returns>
    public LanguageInfoDto Create(
        string? languageCode = null,
        string? languageName = null,
        string? nativeName = null)
    {
        return new LanguageInfoDto
        {
            LanguageCode = languageCode ?? _faker.Random.String2(2, "abcdefghijklmnopqrstuvwxyz"),
            LanguageName = languageName ?? _faker.Address.Country(),
            NativeName = nativeName ?? _faker.Lorem.Word()
        };
    }

    /// <summary>
    /// Creates multiple <see cref="LanguageInfoDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LanguageInfoDto"/> instances.</returns>
    public List<LanguageInfoDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
