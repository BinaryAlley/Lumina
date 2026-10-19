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
        public const string ARTISTS_INDEX = "{culture}/library/audio-library/music-library/artists";
        public const string GET_ARTISTS = "{culture}/library/audio-library/music-library/artists/api-get-artists";
        public const string ARTIST = "{culture}/library/audio-library/music-library/artists/{artistId}";
        public const string GET_ARTIST = "{culture}/library/audio-library/music-library/artists/{artistId}/api-get-artist";
        public const string SAVE_ARTIST = "{culture}/library/audio-library/music-library/artists/{artistId}/api-save-artist";
        public const string UPDATE_ARTIST_COVER = "{culture}/library/audio-library/music-library/artists/{artistId}/api-update-cover";
        public const string DELETE_ARTIST = "{culture}/library/audio-library/music-library/artists/{artistId}/api-delete-artist";
        public const string GET_ARTIST_ALBUMS = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/api-get-albums";
        public const string ALBUM = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}";
        public const string GET_ALBUM = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/api-get-album";
        public const string SAVE_ALBUM = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/api-save-album";
        public const string UPDATE_ALBUM_COVER = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/api-update-cover";
        public const string DELETE_ALBUM = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/api-delete-album";
        public const string GET_ALBUM_TRACKS = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/api-get-tracks";
        public const string SHUFFLE_TRACKS = "{culture}/library/audio-library/music-library/tracks/api-shuffle-tracks";
        public const string TRACK = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}";
        public const string GET_TRACK = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-get-track";
        public const string SAVE_TRACK = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-save-track";
        public const string DELETE_TRACK = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-delete-track";
        public const string STREAM_TRACK = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-stream-track";
        public const string STREAM_TRACK_SEGMENT = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-stream-track-segment/{deliveryId}/{segmentIndex}";
        public const string GET_PLAYBACK_MANIFEST = "{culture}/library/audio-library/music-library/artists/{artistId}/albums/{albumId}/tracks/{trackId}/api-get-playback-manifest";
    }
}
