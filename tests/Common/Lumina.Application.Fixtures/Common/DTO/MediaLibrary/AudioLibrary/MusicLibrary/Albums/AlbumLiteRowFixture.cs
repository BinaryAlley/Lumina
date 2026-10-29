#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Fixture class for the <see cref="AlbumLiteRow"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumLiteRowFixture
{
    private const int MINIMUM_RELEASE_YEAR = 1900;
    private const int MAXIMUM_RELEASE_YEAR = 2026;

    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="AlbumLiteRow"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the album.</param>
    /// <param name="title">Optional. The title of the album.</param>
    /// <param name="totalTracks">Optional. The number of tracks of the release.</param>
    /// <param name="coverPath">Optional. The path of the image representing the album.</param>
    /// <param name="releaseYear">Optional. The release year of the album.</param>
    /// <param name="releaseTypes">Optional. The set of types of the release.</param>
    /// <param name="includeCoverPath">Whether the cover path should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReleaseYear">Whether the release year should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReleaseTypes">Whether the release types should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="AlbumLiteRow"/>.</returns>
    public AlbumLiteRow Create(
        Guid? id = null,
        string? title = null,
        int? totalTracks = null,
        string? coverPath = null,
        int? releaseYear = null,
        List<MusicReleaseType>? releaseTypes = null,
        bool includeCoverPath = true,
        bool includeReleaseYear = true,
        bool includeReleaseTypes = true)
    {
        return new AlbumLiteRow(
            id ?? _faker.Random.Guid(),
            title ?? _faker.Music.Genre(),
            totalTracks ?? _faker.Random.Int(1, 30),
            includeCoverPath ? (coverPath ?? _faker.System.FilePath()) : null,
            includeReleaseYear ? (releaseYear ?? _faker.Random.Int(MINIMUM_RELEASE_YEAR, MAXIMUM_RELEASE_YEAR)) : null,
            includeReleaseTypes ? (releaseTypes ?? [.. Enumerable.Range(0, _faker.Random.Int(1, 3)).Select(index => _faker.PickRandom<MusicReleaseType>())]) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="AlbumLiteRow"/>.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AlbumLiteRow"/> instances.</returns>
    public List<AlbumLiteRow> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
