#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="MusicAlbumLiteDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicAlbumLiteDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MusicAlbumLiteDto"/> instance with randomized test data.
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
    /// <returns>A configured <see cref="MusicAlbumLiteDto"/> instance.</returns>
    public MusicAlbumLiteDto Create(
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
        return new MusicAlbumLiteDto
        {
            Id = id ?? _faker.Random.Guid(),
            Title = title ?? _faker.Lorem.Sentence(3),
            TotalTracks = totalTracks ?? _faker.Random.Number(1, 30),
            CoverPath = includeCoverPath ? (coverPath ?? _faker.System.FilePath()) : null,
            ReleaseYear = includeReleaseYear ? (releaseYear ?? Random.Shared.Next(1900, 2026)) : null,
            ReleaseTypes = includeReleaseTypes ? (releaseTypes ?? [.. Enumerable.Range(0, _faker.Random.Number(1, 3)).Select(_ => _faker.PickRandom<MusicReleaseType>())]) : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicAlbumLiteDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicAlbumLiteDto"/> instances.</returns>
    public List<MusicAlbumLiteDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
