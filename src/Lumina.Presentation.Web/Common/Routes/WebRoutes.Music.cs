namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of routes defined in the Web application.
/// </summary>
public static partial class WebRoutes
{
    /// <summary>
    /// Routes for the music library pages.
    /// </summary>
    public static class Music
    {
        /// <summary>
        /// The route for the artists page of the music library.
        /// </summary>
        public const string ARTISTS_INDEX = "{culture}/library/audio-library/music-library/artists";

        /// <summary>
        /// The route for the deleted music items page.
        /// </summary>
        public const string DELETED_MUSIC_ITEMS = "{culture}/library/audio-library/music-library/deleted-items";

        /// <summary>
        /// The route for the action that retrieves the artists of the music library.
        /// </summary>
        public const string GET_ARTISTS = "{culture}/library/audio-library/music-library/artists/api-get-artists";

        /// <summary>
        /// The route for the artist editing page.
        /// </summary>
        public const string ARTIST = "{culture}/library/audio-library/music-library/artists/{artistId}";

        /// <summary>
        /// The route for the action that retrieves the details of an artist.
        /// </summary>
        public const string GET_ARTIST = "{culture}/library/audio-library/music-library/artists/{artistId}/api-get-artist";

        /// <summary>
        /// The route for the action that saves an artist.
        /// </summary>
        public const string SAVE_ARTIST = "{culture}/library/audio-library/music-library/artists/{artistId}/api-save-artist";

        /// <summary>
        /// The route for the action that updates the cover of an artist.
        /// </summary>
        public const string UPDATE_ARTIST_COVER = "{culture}/library/audio-library/music-library/artists/{artistId}/api-update-cover";

        /// <summary>
        /// The route for the action that deletes an artist.
        /// </summary>
        public const string DELETE_ARTIST = "{culture}/library/audio-library/music-library/artists/{artistId}/api-delete-artist";

        /// <summary>
        /// The route for the action that restores a deleted artist.
        /// </summary>
        public const string RESTORE_ARTIST = "{culture}/library/audio-library/music-library/artists/{artistId}/api-restore-artist";

        /// <summary>
        /// The route for the action that retrieves the albums of an artist.
        /// </summary>
        public const string GET_ARTIST_ALBUMS = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/api-get-albums";

        /// <summary>
        /// The route for the album editing page.
        /// </summary>
        public const string ALBUM = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}";

        /// <summary>
        /// The route for the action that retrieves the details of an album.
        /// </summary>
        public const string GET_ALBUM = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/api-get-album";

        /// <summary>
        /// The route for the action that saves an album.
        /// </summary>
        public const string SAVE_ALBUM = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/api-save-album";

        /// <summary>
        /// The route for the action that updates the cover of an album.
        /// </summary>
        public const string UPDATE_ALBUM_COVER = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/api-update-cover";

        /// <summary>
        /// The route for the action that deletes an album.
        /// </summary>
        public const string DELETE_ALBUM = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/api-delete-album";

        /// <summary>
        /// The route for the action that restores a deleted album.
        /// </summary>
        public const string RESTORE_ALBUM = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/api-restore-album";

        /// <summary>
        /// The route for the action that retrieves the tracks of an album.
        /// </summary>
        public const string GET_ALBUM_TRACKS = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/api-get-tracks";

        /// <summary>
        /// The route for the action that shuffles the tracks of the music library.
        /// </summary>
        public const string SHUFFLE_TRACKS = "{culture}/library/audio-library/music-library/tracks/api-shuffle-tracks";

        /// <summary>
        /// The route for the track editing page.
        /// </summary>
        public const string TRACK = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}";

        /// <summary>
        /// The route for the action that retrieves the details of a track.
        /// </summary>
        public const string GET_TRACK = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-get-track";

        /// <summary>
        /// The route for the action that saves a track.
        /// </summary>
        public const string SAVE_TRACK = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-save-track";

        /// <summary>
        /// The route for the action that deletes a track.
        /// </summary>
        public const string DELETE_TRACK = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-delete-track";

        /// <summary>
        /// The route for the action that restores a deleted track.
        /// </summary>
        public const string RESTORE_TRACK = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-restore-track";

        /// <summary>
        /// The route for the action that streams a track.
        /// </summary>
        public const string STREAM_TRACK = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-stream-track";

        /// <summary>
        /// The route for the action that streams a segment of a track delivery.
        /// </summary>
        public const string STREAM_TRACK_SEGMENT = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-stream-track-segment/{deliveryId}/{segmentIndex}";

        /// <summary>
        /// The route for the action that retrieves the playback manifest of a track.
        /// </summary>
        public const string GET_PLAYBACK_MANIFEST = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-get-playback-manifest";
    }
}
