#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.AudioLibrary.MusicLibrary.Albums;

/// <summary>
/// Fixture class for the <see cref="GetArtistAlbumsLiteRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetArtistAlbumsLiteRequestFixture
{
    /// <summary>
    /// Creates a new <see cref="GetArtistAlbumsLiteRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="artistId">Optional Id of the artist whose albums are retrieved.</param>
    /// <param name="libraryId">Optional Id of the media library the artist belongs to.</param>
    /// <param name="currentPage">Optional page of results to retrieve.</param>
    /// <param name="perPage">Optional maximum number of albums to retrieve per page.</param>
    /// <returns>A configured <see cref="GetArtistAlbumsLiteRequest"/> instance.</returns>
    public GetArtistAlbumsLiteRequest Create(
        string? artistId = null,
        Guid? libraryId = null,
        int? currentPage = null,
        int? perPage = null)
    {
        return new GetArtistAlbumsLiteRequest(
            ArtistId: artistId ?? Guid.NewGuid().ToString(),
            LibraryId: libraryId ?? Guid.NewGuid(),
            CurrentPage: currentPage,
            PerPage: perPage
        );
    }

    /// <summary>
    /// Creates multiple <see cref="GetArtistAlbumsLiteRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="GetArtistAlbumsLiteRequest"/> instances.</returns>
    public List<GetArtistAlbumsLiteRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
