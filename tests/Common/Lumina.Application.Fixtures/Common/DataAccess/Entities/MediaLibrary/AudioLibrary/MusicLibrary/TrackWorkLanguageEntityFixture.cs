#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="TrackWorkLanguageEntity"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackWorkLanguageEntityFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="TrackWorkLanguageEntity"/>.
    /// </summary>
    /// <param name="languageCode">Optional. The ISO 639-1 two-letter language code.</param>
    /// <param name="languageName">Optional. The full name of the language in English.</param>
    /// <param name="nativeName">Optional. The native name of the language.</param>
    /// <param name="includeNativeName">Whether the native name should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="TrackWorkLanguageEntity"/>.</returns>
    public TrackWorkLanguageEntity Create(
        string? languageCode = null,
        string? languageName = null,
        string? nativeName = null,
        bool includeNativeName = true)
    {
        return new TrackWorkLanguageEntity(
            languageCode ?? _faker.PickRandom("en", "fr", "de", "es", "it", "ja", "ko", "zh", "pt", "ru"),
            languageName ?? _faker.Lorem.Word(),
            includeNativeName ? (nativeName ?? _faker.Lorem.Word()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="TrackWorkLanguageEntity"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<TrackWorkLanguageEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
