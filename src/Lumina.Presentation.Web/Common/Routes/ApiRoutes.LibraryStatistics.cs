namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the consumption history, consumption statistics, and resume points of the remote API.
    /// </summary>
    public static class LibraryStatistics
    {
        public const string GET_RECENTLY_CONSUMED = "users/me/consumption-history/recently-consumed";
        public const string GET_MOST_CONSUMED = "users/me/consumption-history/most-consumed";
        public const string GET_CONSUMPTION_STATISTICS = "users/me/consumption-statistics";
        public const string GET_TAG_CONSUMPTION = "users/me/consumption-statistics/tags";
        public const string GET_GENRE_CONSUMPTION = "users/me/consumption-statistics/genres";
        public const string GET_USER_RECENTLY_CONSUMED = "users/{userId}/consumption-history/recently-consumed";
        public const string GET_USER_MOST_CONSUMED = "users/{userId}/consumption-history/most-consumed";
        public const string GET_USER_CONSUMPTION_STATISTICS = "users/{userId}/consumption-statistics";
        public const string GET_USER_TAG_CONSUMPTION = "users/{userId}/consumption-statistics/tags";
        public const string GET_USER_GENRE_CONSUMPTION = "users/{userId}/consumption-statistics/genres";
        public const string GET_ALL_USERS_CONSUMPTION_STATISTICS = "users/consumption-statistics";
        public const string DELETE_CONSUMPTION_ITEM_HISTORY = "users/me/consumption-history/media-items/{mediaKind}/{mediaItemId}/{libraryId}";
        public const string DELETE_USER_CONSUMPTION_ITEM_HISTORY = "users/{userId}/consumption-history/media-items/{mediaKind}/{mediaItemId}/{libraryId}";
        public const string CLEAR_CONSUMPTION_HISTORY = "users/me/consumption-history";
        public const string CLEAR_USER_CONSUMPTION_HISTORY = "users/{userId}/consumption-history";
        public const string CLEAR_RESUME_POINTS = "users/me/media-resume-points";
        public const string CLEAR_USER_RESUME_POINTS = "users/{userId}/media-resume-points";
    }
}
