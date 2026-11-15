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
/// Fixture class for the <see cref="DeletedMusicArtistDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DeletedMusicArtistDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="DeletedMusicArtistDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The Id of the deleted artist.</param>
    /// <param name="libraryId">Optional. The Id of the media library the deleted artist belongs to.</param>
    /// <param name="name">Optional. The name of the deleted artist.</param>
    /// <param name="deletedOnUtc">Optional. The date and time when the artist was deleted.</param>
    /// <param name="coverPath">Optional. The relative path of the stored cover image of the deleted artist.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="DeletedMusicArtistDto"/>.</returns>
    public DeletedMusicArtistDto Create(
        Guid? id = null,
        Guid? libraryId = null,
        string? name = null,
        DateTime? deletedOnUtc = null,
        string? coverPath = null,
        bool includeCoverPath = true)
    {
        return new DeletedMusicArtistDto(
            Id: id ?? _faker.Random.Guid(),
            LibraryId: libraryId ?? _faker.Random.Guid(),
            Name: name ?? _faker.Lorem.Word(),
            DeletedOnUtc: deletedOnUtc ?? _faker.Date.Recent(),
            CoverPath: includeCoverPath ? (coverPath ?? _faker.System.FilePath()) : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="DeletedMusicArtistDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DeletedMusicArtistDto"/> instances.</returns>
    public List<DeletedMusicArtistDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
