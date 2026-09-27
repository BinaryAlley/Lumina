#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
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
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="AlbumLiteRow"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the album.</param>
    /// <param name="title">Optional. The title of the album.</param>
    /// <param name="totalTracks">Optional. The number of tracks of the release.</param>
    /// <returns>The created <see cref="AlbumLiteRow"/>.</returns>
    public AlbumLiteRow Create(
        Guid? id = null,
        string? title = null,
        int? totalTracks = null)
    {
        return new AlbumLiteRow
        {
            Id = id ?? _faker.Random.Guid(),
            Title = title ?? _faker.Music.Genre(),
            TotalTracks = totalTracks ?? _faker.Random.Int(1, 30)
        };
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
