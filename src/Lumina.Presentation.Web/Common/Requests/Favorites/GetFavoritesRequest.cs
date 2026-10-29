#region ========================================================================= USING =====================================================================================
using Lumina.Presentation.Web.Common.Enums.Favorites;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Favorites;

/// <summary>
/// Represents a request for getting a page of the favorites of the current user.
/// </summary>
/// <param name="MediaKind">The kind of media item to restrict the favorites to. Optional; every kind is returned when omitted.</param>
/// <param name="CurrentPage">The one based page of favorites to return. Optional; defaults to 1.</param>
/// <param name="PerPage">The number of favorites per page. Optional; defaults to 200.</param>
[DebuggerDisplay("MediaKind: {MediaKind}; CurrentPage: {CurrentPage}; PerPage: {PerPage}")]
public record GetFavoritesRequest(
    FavoriteMediaKind? MediaKind,
    int? CurrentPage,
    int? PerPage
);
