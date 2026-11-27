#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.SharedKernel.Common.Enums.Favorites;
using DomainErrors = Lumina.Domain.Common.Errors.Errors;
#endregion

namespace Lumina.Application.Common.Utilities;

/// <summary>
/// Extension methods for the <see cref="FavoriteMediaKind"/> enumeration.
/// </summary>
public static class FavoriteMediaKindExtensions
{
    /// <summary>
    /// Gets the error that reports that a media item of <paramref name="mediaKind"/> does not exist.
    /// </summary>
    /// <param name="mediaKind">The kind of the media item that was not found.</param>
    /// <returns>The not found error specific to the media kind, or the not supported error when the media kind cannot be favorited yet.</returns>
    public static Error ToMediaItemNotFoundError(this FavoriteMediaKind mediaKind)
    {
        return mediaKind switch
        {
            FavoriteMediaKind.MusicArtist => DomainErrors.Music.ArtistNotFound,
            FavoriteMediaKind.MusicAlbum => DomainErrors.Music.AlbumNotFound,
            FavoriteMediaKind.MusicTrack => DomainErrors.Music.TrackNotFound,
            FavoriteMediaKind.Book => DomainErrors.WrittenContent.BookNotFound,
            _ => DomainErrors.Favorites.FavoriteMediaKindIsNotSupported
        };
    }
}
