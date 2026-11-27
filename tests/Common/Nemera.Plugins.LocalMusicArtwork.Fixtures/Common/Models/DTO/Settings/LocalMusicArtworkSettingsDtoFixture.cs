#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Plugins.LocalMusicArtwork.Common.Models.DTO.Settings;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Plugins.LocalMusicArtwork.Fixtures.Common.Models.DTO.Settings;

/// <summary>
/// Fixture class for the <see cref="LocalMusicArtworkSettingsDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class LocalMusicArtworkSettingsDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="LocalMusicArtworkSettingsDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="shouldExtractEmbeddedCover">Optional. Whether the cover embedded in the audio files is extracted when the album has no cover image on disk.</param>
    /// <returns>A configured <see cref="LocalMusicArtworkSettingsDto"/> instance.</returns>
    public LocalMusicArtworkSettingsDto Create(
        bool? shouldExtractEmbeddedCover = null)
    {
        return new LocalMusicArtworkSettingsDto
        {
            ShouldExtractEmbeddedCover = shouldExtractEmbeddedCover ?? _faker.Random.Bool()
        };
    }

    /// <summary>
    /// Creates multiple <see cref="LocalMusicArtworkSettingsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LocalMusicArtworkSettingsDto"/> instances.</returns>
    public List<LocalMusicArtworkSettingsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
