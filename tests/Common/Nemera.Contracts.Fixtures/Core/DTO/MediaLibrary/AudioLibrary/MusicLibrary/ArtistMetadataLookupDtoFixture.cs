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
/// Fixture class for the <see cref="ArtistMetadataLookupDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ArtistMetadataLookupDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ArtistMetadataLookupDto"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the artist belongs to.</param>
    /// <param name="path">Optional. The file system path of a track of the artist.</param>
    /// <param name="musicBrainzArtistId">Optional. The MusicBrainz identifier of the artist.</param>
    /// <param name="name">Optional. The name of the artist.</param>
    /// <returns>The created <see cref="ArtistMetadataLookupDto"/>.</returns>
    public ArtistMetadataLookupDto Create(
        Guid? libraryId = null,
        string? path = null,
        Guid? musicBrainzArtistId = null,
        string? name = null)
    {
        return new ArtistMetadataLookupDto(
            libraryId ?? Guid.NewGuid(),
            path ?? _faker.System.FilePath(),
            musicBrainzArtistId,
            name);
    }

    /// <summary>
    /// Creates a list of <see cref="ArtistMetadataLookupDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ArtistMetadataLookupDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
