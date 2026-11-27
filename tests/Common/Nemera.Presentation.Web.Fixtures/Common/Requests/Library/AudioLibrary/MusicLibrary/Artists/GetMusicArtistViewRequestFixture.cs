#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Fixture class for the <see cref="GetMusicArtistViewRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMusicArtistViewRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="GetMusicArtistViewRequest"/> instance.
    /// </summary>
    /// <param name="artistId">Optional unique identifier of the artist to retrieve.</param>
    /// <param name="libraryId">Optional unique identifier of the media library the artist belongs to.</param>
    /// <returns>A configured <see cref="GetMusicArtistViewRequest"/> instance.</returns>
    public GetMusicArtistViewRequest Create(
        string? artistId = null,
        Guid? libraryId = null)
    {
        return new GetMusicArtistViewRequest(
            artistId ?? Guid.NewGuid().ToString(),
            libraryId ?? Guid.NewGuid());
    }

    /// <summary>
    /// Creates a list of <see cref="GetMusicArtistViewRequest"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetMusicArtistViewRequest"/> instances.</returns>
    public List<GetMusicArtistViewRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
