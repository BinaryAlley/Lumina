#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DTO.Pagination;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistAlbumsLite;

/// <summary>
/// Query for getting the lightweight read models of all the albums of an artist.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library the artist belongs to, taken from the route.</param>
/// <param name="ArtistId">The unique identifier of the artist whose albums are retrieved, taken from the route.</param>
/// <param name="PaginationData">The object containing the requested pagination data.</param>
[DebuggerDisplay("ArtistId: {ArtistId}")]
public record GetArtistAlbumsLiteQuery(
    string? LibraryId,
    string? ArtistId,
    PaginationDataDto? PaginationData
) : IQuery;
