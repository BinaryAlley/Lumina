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
        public const string GET_STATISTICS = "{culture}/library/statistics/api-get-statistics";
        public const string GET_ALL_USERS_STATISTICS = "{culture}/library/statistics/api-get-all-users-statistics";
        public const string GET_RECENTLY_CONSUMED = "{culture}/library/statistics/api-get-recently-consumed";
        public const string GET_MOST_CONSUMED = "{culture}/library/statistics/api-get-most-consumed";
        public const string GET_TAG_CONSUMPTION = "{culture}/library/statistics/api-get-tag-consumption";
        public const string GET_GENRE_CONSUMPTION = "{culture}/library/statistics/api-get-genre-consumption";
        public const string GET_LIBRARIES = "{culture}/library/statistics/api-get-libraries";
        public const string GET_USERS = "{culture}/library/statistics/api-get-users";
        public const string DELETE_CONSUMPTION_ITEM_HISTORY = "{culture}/library/statistics/api-delete-consumption-item-history";
        public const string CLEAR_CONSUMPTION_HISTORY = "{culture}/library/statistics/api-clear-consumption-history";
        public const string CLEAR_RESUME_POINTS = "{culture}/library/statistics/api-clear-resume-points";
    }
}
