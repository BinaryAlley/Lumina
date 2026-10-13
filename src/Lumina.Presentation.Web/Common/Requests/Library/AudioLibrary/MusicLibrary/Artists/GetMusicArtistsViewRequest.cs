#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Presentation.Web.Common.Requests.Library.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Represents a request for displaying the music artists browsing view.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library whose artists are browsed. Optional.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public record GetMusicArtistsViewRequest(
    Guid? LibraryId
);
