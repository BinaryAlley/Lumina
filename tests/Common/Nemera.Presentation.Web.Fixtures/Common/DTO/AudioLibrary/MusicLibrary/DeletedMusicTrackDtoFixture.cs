#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="DeletedMusicTrackDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletedMusicTrackDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="DeletedMusicTrackDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The Id of the deleted track.</param>
    /// <param name="libraryId">Optional. The Id of the media library the deleted track belongs to.</param>
    /// <param name="artistId">Optional. The Id of the artist the deleted track belongs to.</param>
    /// <param name="artistName">Optional. The name of the artist the deleted track belongs to.</param>
    /// <param name="albumId">Optional. The Id of the album the deleted track belongs to.</param>
    /// <param name="albumTitle">Optional. The title of the album the deleted track belongs to.</param>
    /// <param name="title">Optional. The title of the deleted track.</param>
    /// <param name="path">Optional. The file system path of the deleted track.</param>
    /// <param name="deletedOnUtc">Optional. The date and time when the track was deleted.</param>
    /// <returns>The created <see cref="DeletedMusicTrackDto"/>.</returns>
    public DeletedMusicTrackDto Create(
        Guid? id = null,
        Guid? libraryId = null,
        Guid? artistId = null,
        string? artistName = null,
        Guid? albumId = null,
        string? albumTitle = null,
        string? title = null,
        string? path = null,
        DateTime? deletedOnUtc = null)
    {
        return new DeletedMusicTrackDto(
            Id: id ?? _faker.Random.Guid(),
            LibraryId: libraryId ?? _faker.Random.Guid(),
            ArtistId: artistId ?? _faker.Random.Guid(),
            ArtistName: artistName ?? _faker.Lorem.Word(),
            AlbumId: albumId ?? _faker.Random.Guid(),
            AlbumTitle: albumTitle ?? _faker.Lorem.Sentence(),
            Title: title ?? _faker.Lorem.Sentence(),
            Path: path ?? _faker.System.FilePath(),
            DeletedOnUtc: deletedOnUtc ?? _faker.Date.Recent()
        );
    }

    /// <summary>
    /// Creates multiple <see cref="DeletedMusicTrackDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DeletedMusicTrackDto"/> instances.</returns>
    public List<DeletedMusicTrackDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
