#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Fixture class for the <see cref="AlbumLiteResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumLiteResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="AlbumLiteResponse"/>.
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
    /// <returns>The created <see cref="AlbumLiteResponse"/>.</returns>
    public AlbumLiteResponse Create(
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
        List<MusicReleaseType>? resolvedReleaseTypes = includeReleaseTypes
            ? (releaseTypes ?? [.. Enumerable.Range(0, _faker.Random.Int(1, 3)).Select(_ => _faker.PickRandom<MusicReleaseType>())])
            : null;

        return new AlbumLiteResponse(
            id ?? Guid.NewGuid(),
            title ?? _faker.Commerce.ProductName(),
            totalTracks ?? _faker.Random.Int(1, 30),
            includeCoverPath ? (coverPath ?? _faker.System.FilePath()) : null,
            includeReleaseYear ? (releaseYear ?? _faker.Random.Int(1900, 2026)) : null,
            resolvedReleaseTypes);
    }

    /// <summary>
    /// Creates a list of <see cref="AlbumLiteResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<AlbumLiteResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
