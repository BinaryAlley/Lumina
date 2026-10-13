#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Represents a request for displaying the details of a music artist.
/// </summary>
/// <param name="ArtistId">The unique identifier of the artist whose details are displayed, taken from the route. Required.</param>
/// <param name="LibraryId">The Id of the media library the artist belongs to. Required.</param>
[DebuggerDisplay("ArtistId: {ArtistId}")]
public record GetMusicArtistViewRequest(
    string ArtistId,
    Guid LibraryId
);
