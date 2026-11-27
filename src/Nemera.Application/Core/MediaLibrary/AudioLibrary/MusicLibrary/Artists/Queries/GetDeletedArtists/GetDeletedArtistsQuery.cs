#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artists.Queries.GetDeletedArtists;

/// <summary>
/// Query for getting the deleted artists of a media library.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library whose deleted artists are retrieved, taken from the route.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public record GetDeletedArtistsQuery(
    string? LibraryId
) : IQuery;
