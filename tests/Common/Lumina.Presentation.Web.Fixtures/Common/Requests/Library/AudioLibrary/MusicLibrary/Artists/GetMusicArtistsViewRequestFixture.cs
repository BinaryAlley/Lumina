#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Fixture class for the <see cref="GetMusicArtistsViewRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetMusicArtistsViewRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="GetMusicArtistsViewRequest"/> instance.
    /// </summary>
    /// <param name="libraryId">Optional unique identifier of the media library whose artists are browsed.</param>
    /// <returns>A configured <see cref="GetMusicArtistsViewRequest"/> instance.</returns>
    public GetMusicArtistsViewRequest Create(
        Guid? libraryId = null)
    {
        return new GetMusicArtistsViewRequest(
            LibraryId: libraryId
        );
    }

    /// <summary>
    /// Creates multiple <see cref="GetMusicArtistsViewRequest"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetMusicArtistsViewRequest"/> instances.</returns>
    public List<GetMusicArtistsViewRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
