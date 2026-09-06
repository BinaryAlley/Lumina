#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.Common;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.Common;

/// <summary>
/// Fixture class for the <see cref="LanguageInfoDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LanguageInfoDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="LanguageInfoDto"/>.
    /// </summary>
    /// <param name="languageCode">Optional. The ISO code of the language.</param>
    /// <param name="languageName">Optional. The name of the language in English.</param>
    /// <param name="nativeName">Optional. The native name of the language.</param>
    /// <param name="includeLanguageCode">Whether the language code should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLanguageName">Whether the language name should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeNativeName">Whether the native name should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="LanguageInfoDto"/>.</returns>
    public LanguageInfoDto Create(
        string? languageCode = null,
        string? languageName = null,
        string? nativeName = null,
        bool includeLanguageCode = true,
        bool includeLanguageName = true,
        bool includeNativeName = true)
    {
        return new LanguageInfoDto(
            includeLanguageCode ? (languageCode ?? _faker.Random.String2(2)) : null,
            includeLanguageName ? (languageName ?? _faker.Lorem.Word()) : null,
            includeNativeName ? (nativeName ?? _faker.Lorem.Word()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="LanguageInfoDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<LanguageInfoDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
