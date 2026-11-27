#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.Favorites;

/// <summary>
/// Represents a request for getting a window of the favorites of the current user.
/// </summary>
/// <param name="MediaKind">The kind of media item to restrict the favorites to. Optional; every kind is returned when omitted.</param>
/// <param name="CurrentPage">The one based page of favorites to return. Optional; defaults to 1 when a page size is provided.</param>
/// <param name="PerPage">The number of favorites per page. Optional; defaults to 200 when a page is provided. When both pagination parameters are omitted, every matching favorite is returned.</param>
[DebuggerDisplay("MediaKind: {MediaKind}; CurrentPage: {CurrentPage}; PerPage: {PerPage}")]
public record GetFavoritesRequest(
    FavoriteMediaKind? MediaKind,
    int? CurrentPage,
    int? PerPage
);
