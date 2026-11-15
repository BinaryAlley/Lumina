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
/// Fixture class for the <see cref="DeletedMusicAlbumDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletedMusicAlbumDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="DeletedMusicAlbumDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The Id of the deleted album.</param>
    /// <param name="libraryId">Optional. The Id of the media library the deleted album belongs to.</param>
    /// <param name="artistId">Optional. The Id of the artist the deleted album belongs to.</param>
    /// <param name="artistName">Optional. The name of the artist the deleted album belongs to.</param>
    /// <param name="title">Optional. The title of the deleted album.</param>
    /// <param name="deletedOnUtc">Optional. The date and time when the album was deleted.</param>
    /// <param name="coverPath">Optional. The relative path of the stored cover image of the deleted album.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="DeletedMusicAlbumDto"/>.</returns>
    public DeletedMusicAlbumDto Create(
        Guid? id = null,
        Guid? libraryId = null,
        Guid? artistId = null,
        string? artistName = null,
        string? title = null,
        DateTime? deletedOnUtc = null,
        string? coverPath = null,
        bool includeCoverPath = true)
    {
        return new DeletedMusicAlbumDto(
            Id: id ?? _faker.Random.Guid(),
            LibraryId: libraryId ?? _faker.Random.Guid(),
            ArtistId: artistId ?? _faker.Random.Guid(),
            ArtistName: artistName ?? _faker.Lorem.Word(),
            Title: title ?? _faker.Lorem.Sentence(),
            DeletedOnUtc: deletedOnUtc ?? _faker.Date.Recent(),
            CoverPath: includeCoverPath ? (coverPath ?? _faker.System.FilePath()) : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="DeletedMusicAlbumDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DeletedMusicAlbumDto"/> instances.</returns>
    public List<DeletedMusicAlbumDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
