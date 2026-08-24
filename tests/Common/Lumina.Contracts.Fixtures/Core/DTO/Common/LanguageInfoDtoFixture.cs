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
    /// <returns>The created <see cref="LanguageInfoDto"/>.</returns>
    public LanguageInfoDto Create(
        string? languageCode = null, 
        string? languageName = null, 
        string? nativeName = null)
    {
        return new LanguageInfoDto(
            languageCode ?? _faker.Random.String2(2),
            languageName ?? _faker.Lorem.Word(),
            nativeName ?? _faker.Lorem.Word());
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
