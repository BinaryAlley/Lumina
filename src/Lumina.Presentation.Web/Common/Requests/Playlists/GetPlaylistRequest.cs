#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Playlists;

/// <summary>
/// Represents a request for getting a page of the entries of a playlist of the current user.
/// </summary>
/// <param name="CurrentPage">The one based page of entries to return. Optional; defaults to 1.</param>
/// <param name="PerPage">The number of entries per page. Optional; defaults to 200.</param>
[DebuggerDisplay("CurrentPage: {CurrentPage}; PerPage: {PerPage}")]
public record GetPlaylistRequest(
    int? CurrentPage,
    int? PerPage
);
