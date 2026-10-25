namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the music play statistics endpoints of the remote API.
    /// </summary>
    public static class PlayStatistics
    {
        public const string GET_RECENTLY_PLAYED = "users/me/music-play-history/recently-played";
        public const string GET_MOST_PLAYED = "users/me/music-play-history/most-played";
        public const string GET_MUSIC_PLAY_STATISTICS = "users/me/music-play-history/statistics";
    }
}
