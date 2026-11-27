#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Fixture class for the <see cref="GetMusicTrackViewRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMusicTrackViewRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="GetMusicTrackViewRequest"/> instance.
    /// </summary>
    /// <param name="artistId">Optional unique identifier of the artist the album belongs to.</param>
    /// <param name="albumId">Optional unique identifier of the album the track belongs to.</param>
    /// <param name="trackId">Optional unique identifier of the track to retrieve.</param>
    /// <param name="libraryId">Optional unique identifier of the media library the track belongs to.</param>
    /// <returns>A configured <see cref="GetMusicTrackViewRequest"/> instance.</returns>
    public GetMusicTrackViewRequest Create(
        string? artistId = null,
        string? albumId = null,
        string? trackId = null,
        Guid? libraryId = null)
    {
        return new GetMusicTrackViewRequest(
            artistId ?? Guid.NewGuid().ToString(),
            albumId ?? Guid.NewGuid().ToString(),
            trackId ?? Guid.NewGuid().ToString(),
            libraryId ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="GetMusicTrackViewRequest"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetMusicTrackViewRequest"/> instances.</returns>
    public List<GetMusicTrackViewRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
