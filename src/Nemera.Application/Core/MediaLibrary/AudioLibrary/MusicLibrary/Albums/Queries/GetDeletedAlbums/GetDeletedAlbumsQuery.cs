#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.CQRS;
using System.Diagnostics;
#endregion

namespace Nemera.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Albums.Queries.GetDeletedAlbums;

/// <summary>
/// Query for getting the deleted albums of a media library.
/// </summary>
/// <param name="LibraryId">The unique identifier of the media library whose deleted albums are retrieved, taken from the route.</param>
[DebuggerDisplay("LibraryId: {LibraryId}")]
public record GetDeletedAlbumsQuery(
    string? LibraryId
) : IQuery;
