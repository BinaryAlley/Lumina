#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Fixture class for the <see cref="GetPlaybackManifestRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPlaybackManifestRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="GetPlaybackManifestRequest"/> instance.
    /// </summary>
    /// <param name="artistId">Optional unique identifier of the artist the album belongs to.</param>
    /// <param name="albumId">Optional unique identifier of the album the track belongs to.</param>
    /// <param name="trackId">Optional unique identifier of the track whose manifest is requested.</param>
    /// <param name="libraryId">Optional unique identifier of the media library the track belongs to.</param>
    /// <param name="capabilities">Optional capabilities the player advertises it can play directly.</param>
    /// <param name="includeCapabilities">Whether the capabilities should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="GetPlaybackManifestRequest"/> instance.</returns>
    public GetPlaybackManifestRequest Create(
        string? artistId = null,
        string? albumId = null,
        string? trackId = null,
        Guid? libraryId = null,
        string? capabilities = null,
        bool includeCapabilities = true)
    {
        return new GetPlaybackManifestRequest(
            artistId ?? Guid.NewGuid().ToString(),
            albumId ?? Guid.NewGuid().ToString(),
            trackId ?? Guid.NewGuid().ToString(),
            libraryId ?? Guid.NewGuid(),
            includeCapabilities ? (capabilities ?? "mp3/mp3") : null);
    }

    /// <summary>
    /// Creates a list of <see cref="GetPlaybackManifestRequest"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetPlaybackManifestRequest"/> instances.</returns>
    public List<GetPlaybackManifestRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
