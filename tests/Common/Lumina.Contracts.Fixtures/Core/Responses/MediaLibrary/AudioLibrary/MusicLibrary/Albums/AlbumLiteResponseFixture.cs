#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Albums;
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
    /// <returns>The created <see cref="AlbumLiteResponse"/>.</returns>
    public AlbumLiteResponse Create(
        Guid? id = null,
        string? title = null,
        int? totalTracks = null)
    {
        return new AlbumLiteResponse(
            id ?? Guid.NewGuid(),
            title ?? _faker.Commerce.ProductName(),
            totalTracks ?? _faker.Random.Int(1, 30));
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
