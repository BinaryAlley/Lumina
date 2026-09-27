#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DTO.Pagination;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistsLite;

/// <summary>
/// Query for getting the lightweight read models of all the artists of a media library.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library whose artists are retrieved, taken from the route.</param>
/// <param name="PaginationData">The object containing the requested pagination data.</param>
/// <param name="SearchTerm">The search term used to filter the artists by name.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public record GetArtistsLiteQuery(
    string? LibraryId,
    PaginationDataDto? PaginationData,
    string? SearchTerm
) : IQuery;
