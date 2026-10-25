namespace Lumina.Presentation.Api.Common.Routes.Library.AudioLibrary.MusicLibrary;

/// <summary>
/// Class for the collection of the music play statistics routes of the API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the music play statistics of the current user.
    /// </summary>
    public static class PlayStatistics
    {
        public const string GET_RECENTLY_PLAYED = "/users/me/music-play-history/recently-played";
        public const string GET_MOST_PLAYED = "/users/me/music-play-history/most-played";
        public const string GET_MUSIC_PLAY_STATISTICS = "/users/me/music-play-history/statistics";
    }
}
