namespace Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;

/// <summary>
/// Class for the collection of routes defined in this API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the Albums route.
    /// </summary>
    public static class Albums
    {
        public const string GET_ARTIST_ALBUMS = "/libraries/{libraryId}/artists/{artistId}/albums";
        public const string GET_ARTIST_ALBUMS_LITE = "/libraries/{libraryId}/artists/{artistId}/albums/lite";
        public const string ADD_ALBUM = "/libraries/{libraryId}/artists/{artistId}/albums";
        public const string GET_ALBUM_BY_ID = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}";
        public const string UPDATE_ALBUM = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}";
        public const string DELETE_ALBUM = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}";
        public const string GET_ALBUM_TRACKS = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks";
        public const string GET_ALBUM_TRACKS_LITE = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/lite";
    }
}
