#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Application.Core.MediaLibrary.AudioLibrary.MusicLibrary.Artwork;

/// <summary>
/// Interface for the service that removes the artwork records and the stored artwork files of a music library item, together with the artwork of its children.
/// </summary>
public interface IMusicArtworkCleanupService
{
    /// <summary>
    /// Removes the artwork records and the stored artwork files of the artist identified by <paramref name="artistId"/>, together with the artwork of its albums and tracks.
    /// </summary>
    /// <param name="artistId">The Id of the artist whose artwork is removed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Deleted>> DeleteArtistArtworkAsync(Guid artistId, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the artwork records and the stored artwork files of the album identified by <paramref name="albumId"/>, together with the artwork of its tracks.
    /// </summary>
    /// <param name="albumId">The Id of the album whose artwork is removed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Deleted>> DeleteAlbumArtworkAsync(Guid albumId, CancellationToken cancellationToken);

    /// <summary>
    /// Removes the artwork records and the stored artwork files of the track identified by <paramref name="trackId"/>.
    /// </summary>
    /// <param name="trackId">The Id of the track whose artwork is removed.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the execution.</param>
    /// <returns>An <see cref="Result{TValue}"/> representing either a successful operation, or an error.</returns>
    Task<Result<Deleted>> DeleteTrackArtworkAsync(Guid trackId, CancellationToken cancellationToken);
}
