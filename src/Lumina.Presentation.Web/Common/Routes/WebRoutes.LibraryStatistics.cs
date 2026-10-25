namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of routes defined in the Web application.
/// </summary>
public static partial class WebRoutes
{
    /// <summary>
    /// Routes for the library statistics pages.
    /// </summary>
    public static class LibraryStatistics
    {
        public const string INDEX = "{culture}/library/statistics";
        public const string GET_RECENTLY_PLAYED = "{culture}/library/statistics/api-get-recently-played";
        public const string GET_MOST_PLAYED = "{culture}/library/statistics/api-get-most-played";
        public const string GET_MUSIC_PLAY_STATISTICS = "{culture}/library/statistics/api-get-music-play-statistics";
    }
}
