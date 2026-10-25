#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.PlayHistory;

/// <summary>
/// Represents a request for getting a window of the most played tracks of the current user.
/// </summary>
/// <param name="CurrentPage">The one based page of tracks to return. Optional; defaults to 1 when a page size is provided.</param>
/// <param name="PerPage">The number of tracks per page. Optional; defaults to 50 when a page is provided. When both pagination parameters are omitted, every matching track is returned.</param>
[DebuggerDisplay("CurrentPage: {CurrentPage}; PerPage: {PerPage}")]
public record GetMostPlayedRequest(
    int? CurrentPage,
    int? PerPage
);
