#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Fixture class for the <see cref="GetMusicAlbumViewRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMusicAlbumViewRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="GetMusicAlbumViewRequest"/> instance.
    /// </summary>
    /// <param name="artistId">Optional unique identifier of the artist the album belongs to.</param>
    /// <param name="albumId">Optional unique identifier of the album to retrieve.</param>
    /// <param name="libraryId">Optional unique identifier of the media library the album belongs to.</param>
    /// <returns>A configured <see cref="GetMusicAlbumViewRequest"/> instance.</returns>
    public GetMusicAlbumViewRequest Create(
        string? artistId = null,
        string? albumId = null,
        Guid? libraryId = null)
    {
        return new GetMusicAlbumViewRequest(
            artistId ?? Guid.NewGuid().ToString(),
            albumId ?? Guid.NewGuid().ToString(),
            libraryId ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="GetMusicAlbumViewRequest"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetMusicAlbumViewRequest"/> instances.</returns>
    public List<GetMusicAlbumViewRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
