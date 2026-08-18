#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Common.ValueObjects.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Common.ValueObjects.Metadata;

/// <summary>
/// Fixture class for the <see cref="LanguageInfo"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class LanguageInfoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="LanguageInfo"/>.
    /// </summary>
    /// <param name="languageCode">Optional. The ISO 639-1 two-letter language code. If not provided, a random code is generated.</param>
    /// <param name="languageName">Optional. The full name of the language in English. If not provided, a random name is generated.</param>
    /// <param name="nativeName">Optional. The native name of the language. If not provided, a random native name is generated.</param>
    /// <returns>The created <see cref="LanguageInfo"/>.</returns>
    public LanguageInfo Create(
        string? languageCode = null,
        string? languageName = null,
        Optional<string>? nativeName = null)
    {
        string[] languages = ["en", "es", "fr", "de", "it", "ja", "ko", "zh", "pt", "ru"];

        Result<LanguageInfo> languageInfoResult = LanguageInfo.Create(
            languageCode ?? _faker.PickRandom(languages),
            languageName ?? _faker.Lorem.Word(),
            nativeName ?? Optional<string>.Some(_faker.Lorem.Word()));

        if (languageInfoResult.IsFailure)
            throw new InvalidOperationException("Failed to create LanguageInfo: " + string.Join(", ", languageInfoResult.Errors));
        return languageInfoResult.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="LanguageInfo"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LanguageInfo"/> instances.</returns>
    public List<LanguageInfo> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
