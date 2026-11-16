namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the music library endpoints of the remote API.
    /// </summary>
    public static class Music
    {
        /// <summary>
        /// The route for the API endpoint that retrieves the lightweight list of artists of a library.
        /// </summary>
        public const string GET_ARTISTS_LITE = "libraries/{libraryId}/artists/lite";

        /// <summary>
        /// The route for the API endpoint that retrieves an artist of a library.
        /// </summary>
        public const string GET_ARTIST = "libraries/{libraryId}/artists/{artistId}";

        /// <summary>
        /// The route for the API endpoint that updates an artist of a library.
        /// </summary>
        public const string UPDATE_ARTIST = "libraries/{libraryId}/artists/{artistId}";

        /// <summary>
        /// The route for the API endpoint that updates the cover of an artist of a library.
        /// </summary>
        public const string UPDATE_ARTIST_COVER = "libraries/{libraryId}/artists/{artistId}/cover";

        /// <summary>
        /// The route for the API endpoint that deletes an artist of a library.
        /// </summary>
        public const string DELETE_ARTIST = "libraries/{libraryId}/artists/{artistId}";

        /// <summary>
        /// The route for the API endpoint that retrieves the deleted artists of a library.
        /// </summary>
        public const string GET_DELETED_ARTISTS = "libraries/{libraryId}/artists/deleted-items";

        /// <summary>
        /// The route for the API endpoint that restores a deleted artist of a library.
        /// </summary>
        public const string RESTORE_ARTIST = "libraries/{libraryId}/artists/{artistId}/restore";

        /// <summary>
        /// The route for the API endpoint that retrieves the lightweight list of albums of an artist.
        /// </summary>
        public const string GET_ARTIST_ALBUMS_LITE = "libraries/{libraryId}/artists/{artistId}/albums/lite";

        /// <summary>
        /// The route for the API endpoint that retrieves an album of an artist.
        /// </summary>
        public const string GET_ALBUM = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}";

        /// <summary>
        /// The route for the API endpoint that updates an album of an artist.
        /// </summary>
        public const string UPDATE_ALBUM = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}";

        /// <summary>
        /// The route for the API endpoint that updates the cover of an album of an artist.
        /// </summary>
        public const string UPDATE_ALBUM_COVER = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}/cover";

        /// <summary>
        /// The route for the API endpoint that deletes an album of an artist.
        /// </summary>
        public const string DELETE_ALBUM = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}";

        /// <summary>
        /// The route for the API endpoint that retrieves the deleted albums of a library.
        /// </summary>
        public const string GET_DELETED_ALBUMS = "libraries/{libraryId}/albums/deleted-items";

        /// <summary>
        /// The route for the API endpoint that restores a deleted album of an artist.
        /// </summary>
        public const string RESTORE_ALBUM = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}/restore";

        /// <summary>
        /// The route for the API endpoint that retrieves the lightweight list of tracks of an album.
        /// </summary>
        public const string GET_ALBUM_TRACKS_LITE = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/lite";

        /// <summary>
        /// The route for the API endpoint that shuffles the tracks of a library.
        /// </summary>
        public const string SHUFFLE_TRACKS = "libraries/{libraryId}/tracks/shuffle";

        /// <summary>
        /// The route for the API endpoint that retrieves a track of an album.
        /// </summary>
        public const string GET_TRACK = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}";

        /// <summary>
        /// The route for the API endpoint that updates a track of an album.
        /// </summary>
        public const string UPDATE_TRACK = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}";

        /// <summary>
        /// The route for the API endpoint that deletes a track of an album.
        /// </summary>
        public const string DELETE_TRACK = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}";

        /// <summary>
        /// The route for the API endpoint that retrieves the deleted tracks of a library.
        /// </summary>
        public const string GET_DELETED_TRACKS = "libraries/{libraryId}/tracks/deleted-items";

        /// <summary>
        /// The route for the API endpoint that restores a deleted track of an album.
        /// </summary>
        public const string RESTORE_TRACK = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/restore";

        /// <summary>
        /// The route for the API endpoint that streams a track of an album.
        /// </summary>
        public const string STREAM_TRACK = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/stream";

        /// <summary>
        /// The route for the API endpoint that streams a segment of a track delivery.
        /// </summary>
        public const string STREAM_TRACK_SEGMENT = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/stream/deliveries/{deliveryId}/segments/{segmentIndex}";

        /// <summary>
        /// The route for the API endpoint that retrieves the playback manifest of a track.
        /// </summary>
        public const string GET_PLAYBACK_MANIFEST = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}/playback-manifest";
    }
}
