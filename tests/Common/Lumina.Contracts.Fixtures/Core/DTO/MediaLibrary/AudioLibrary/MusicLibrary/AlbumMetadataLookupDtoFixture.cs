#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="AlbumMetadataLookupDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumMetadataLookupDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="AlbumMetadataLookupDto"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the album belongs to.</param>
    /// <param name="path">Optional. The file system path of a track of the album.</param>
    /// <param name="musicBrainzReleaseGroupId">Optional. The MusicBrainz identifier of the release group of the album.</param>
    /// <param name="musicBrainzReleaseId">Optional. The MusicBrainz identifier of the release of the album.</param>
    /// <param name="title">Optional. The title of the album.</param>
    /// <param name="artistName">Optional. The name of the artist of the album.</param>
    /// <param name="releaseYear">Optional. The year the album was released.</param>
    /// <param name="trackCount">Optional. The number of tracks the local album has.</param>
    /// <returns>The created <see cref="AlbumMetadataLookupDto"/>.</returns>
    public AlbumMetadataLookupDto Create(
        Guid? libraryId = null,
        string? path = null,
        Guid? musicBrainzReleaseGroupId = null,
        Guid? musicBrainzReleaseId = null,
        string? title = null,
        string? artistName = null,
        int? releaseYear = null,
        int? trackCount = null)
    {
        return new AlbumMetadataLookupDto(
            libraryId ?? Guid.NewGuid(),
            path ?? _faker.System.FilePath(),
            musicBrainzReleaseGroupId,
            musicBrainzReleaseId,
            title,
            artistName,
            releaseYear,
            trackCount);
    }

    /// <summary>
    /// Creates a list of <see cref="AlbumMetadataLookupDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<AlbumMetadataLookupDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
