namespace Nemera.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;

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
        /// <summary>
        /// The route for getting the albums of an artist in a library.
        /// </summary>
        public const string GET_ARTIST_ALBUMS = "/libraries/{libraryId}/artists/{artistId}/albums";

        /// <summary>
        /// The route for getting the albums of an artist in a library in their lightweight form.
        /// </summary>
        public const string GET_ARTIST_ALBUMS_LITE = "/libraries/{libraryId}/artists/{artistId}/albums/lite";

        /// <summary>
        /// The route for adding an album to an artist in a library.
        /// </summary>
        public const string ADD_ALBUM = "/libraries/{libraryId}/artists/{artistId}/albums";

        /// <summary>
        /// The route for getting an album of an artist in a library.
        /// </summary>
        public const string GET_ALBUM_BY_ID = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}";

        /// <summary>
        /// The route for updating an album of an artist in a library.
        /// </summary>
        public const string UPDATE_ALBUM = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}";

        /// <summary>
        /// The route for updating the cover of an album of an artist in a library.
        /// </summary>
        public const string UPDATE_ALBUM_COVER = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/cover";

        /// <summary>
        /// The route for deleting an album of an artist in a library.
        /// </summary>
        public const string DELETE_ALBUM = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}";

        /// <summary>
        /// The route for getting the deleted albums of a library.
        /// </summary>
        public const string GET_DELETED_ALBUMS = "/libraries/{libraryId}/albums/deleted-items";

        /// <summary>
        /// The route for restoring a deleted album of an artist in a library.
        /// </summary>
        public const string RESTORE_ALBUM = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/restore";

        /// <summary>
        /// The route for getting the tracks of an album of an artist in a library.
        /// </summary>
        public const string GET_ALBUM_TRACKS = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks";

        /// <summary>
        /// The route for getting the tracks of an album of an artist in a library in their lightweight form.
        /// </summary>
        public const string GET_ALBUM_TRACKS_LITE = "/libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/lite";
    }
}
