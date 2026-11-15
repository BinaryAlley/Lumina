#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Tracks.Queries.GetDeletedTracks;

/// <summary>
/// Query for getting the deleted tracks of a media library.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library whose deleted tracks are retrieved, taken from the route.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public record GetDeletedTracksQuery(
    string? LibraryId
) : IQuery;
