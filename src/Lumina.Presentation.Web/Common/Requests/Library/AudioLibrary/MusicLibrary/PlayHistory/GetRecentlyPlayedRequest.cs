#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.PlayHistory;

/// <summary>
/// Represents a request for getting a page of the recently played tracks of the current user.
/// </summary>
/// <param name="CurrentPage">The one based page of tracks to return. Optional. Defaults to 1.</param>
/// <param name="PerPage">The number of tracks per page. Optional. Defaults to 10.</param>
[DebuggerDisplay("CurrentPage: {CurrentPage}; PerPage: {PerPage}")]
public record GetRecentlyPlayedRequest(
    int? CurrentPage,
    int? PerPage
);
