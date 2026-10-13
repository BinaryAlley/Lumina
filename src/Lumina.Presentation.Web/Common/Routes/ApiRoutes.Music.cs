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
        public const string GET_ARTISTS_LITE = "libraries/{libraryId}/artists/lite";
        public const string GET_ARTIST = "libraries/{libraryId}/artists/{artistId}";
        public const string UPDATE_ARTIST = "libraries/{libraryId}/artists/{artistId}";
        public const string UPDATE_ARTIST_COVER = "libraries/{libraryId}/artists/{artistId}/cover";
        public const string DELETE_ARTIST = "libraries/{libraryId}/artists/{artistId}";
        public const string GET_ARTIST_ALBUMS_LITE = "libraries/{libraryId}/artists/{artistId}/albums/lite";
        public const string GET_ALBUM = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}";
        public const string UPDATE_ALBUM = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}";
        public const string UPDATE_ALBUM_COVER = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}/cover";
        public const string DELETE_ALBUM = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}";
        public const string GET_ALBUM_TRACKS_LITE = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/lite";
        public const string GET_TRACK = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}";
        public const string UPDATE_TRACK = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}";
        public const string DELETE_TRACK = "libraries/{libraryId}/artists/{artistId}/albums/{albumId}/tracks/{trackId}";
    }
}
