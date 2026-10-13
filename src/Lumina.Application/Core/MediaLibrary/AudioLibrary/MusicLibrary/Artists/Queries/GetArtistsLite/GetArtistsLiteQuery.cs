#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using Lumina.Application.Common.DTO.Pagination;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetArtistsLite;

/// <summary>
/// Query for getting the lightweight read models of all the artists of a media library.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library whose artists are retrieved, taken from the route.</param>
/// <param name="PaginationData">The object containing the requested pagination data.</param>
/// <param name="SearchTerm">The search term used to filter the artists by name.</param>
/// <param name="FilterAlphaKey">The alpha key used to filter the artists by the first character of their name.</param>
/// <param name="ShouldIgnoreThePrefixForAlphaPicker">Whether the leading "The " prefix of a name should be ignored when computing the alpha key, or not.</param>
/// <param name="SortBy">The name of the field by which to sort the results.</param>
/// <param name="SortOrder">The direction in which to sort the results.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public record GetArtistsLiteQuery(
    string? LibraryId,
    PaginationDataDto? PaginationData,
    string? SearchTerm,
    string? FilterAlphaKey,
    bool ShouldIgnoreThePrefixForAlphaPicker,
    string? SortBy,
    SortOrder? SortOrder
) : IQuery;
