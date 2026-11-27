namespace Lumina.Presentation.Api.Common.Routes.Library.Statistics;

/// <summary>
/// Class for the collection of the library statistics routes of the API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the consumption history, consumption statistics, and resume points of the current user, of another user, and of every user.
    /// </summary>
    public static class LibraryStatistics
    {
        /// <summary>
        /// The route for getting the media items recently consumed by the current user.
        /// </summary>
        public const string GET_RECENTLY_CONSUMED = "/users/me/consumption-history/recently-consumed";

        /// <summary>
        /// The route for getting the most consumed media items of the current user.
        /// </summary>
        public const string GET_MOST_CONSUMED = "/users/me/consumption-history/most-consumed";

        /// <summary>
        /// The route for getting the consumption statistics of the current user.
        /// </summary>
        public const string GET_CONSUMPTION_STATISTICS = "/users/me/consumption-statistics";

        /// <summary>
        /// The route for getting the tag consumption of the current user.
        /// </summary>
        public const string GET_TAG_CONSUMPTION = "/users/me/consumption-statistics/tags";

        /// <summary>
        /// The route for getting the genre consumption of the current user.
        /// </summary>
        public const string GET_GENRE_CONSUMPTION = "/users/me/consumption-statistics/genres";

        /// <summary>
        /// The route for getting the media items recently consumed by a user.
        /// </summary>
        public const string GET_USER_RECENTLY_CONSUMED = "/users/{userId}/consumption-history/recently-consumed";

        /// <summary>
        /// The route for getting the most consumed media items of a user.
        /// </summary>
        public const string GET_USER_MOST_CONSUMED = "/users/{userId}/consumption-history/most-consumed";

        /// <summary>
        /// The route for getting the consumption statistics of a user.
        /// </summary>
        public const string GET_USER_CONSUMPTION_STATISTICS = "/users/{userId}/consumption-statistics";

        /// <summary>
        /// The route for getting the tag consumption of a user.
        /// </summary>
        public const string GET_USER_TAG_CONSUMPTION = "/users/{userId}/consumption-statistics/tags";

        /// <summary>
        /// The route for getting the genre consumption of a user.
        /// </summary>
        public const string GET_USER_GENRE_CONSUMPTION = "/users/{userId}/consumption-statistics/genres";

        /// <summary>
        /// The route for getting the consumption statistics of every user.
        /// </summary>
        public const string GET_ALL_USERS_CONSUMPTION_STATISTICS = "/users/consumption-statistics";

        /// <summary>
        /// The route for deleting a consumption record of the current user.
        /// </summary>
        public const string DELETE_CONSUMPTION_RECORD = "/users/me/consumption-history/{recordId}";

        /// <summary>
        /// The route for deleting a consumption record of a user.
        /// </summary>
        public const string DELETE_USER_CONSUMPTION_RECORD = "/users/{userId}/consumption-history/{recordId}";

        /// <summary>
        /// The route for deleting the consumption history of a media item of the current user in a library.
        /// </summary>
        public const string DELETE_CONSUMPTION_ITEM_HISTORY = "/users/me/consumption-history/media-items/{mediaKind}/{mediaItemId}/{libraryId}";

        /// <summary>
        /// The route for deleting the consumption history of a media item of a user in a library.
        /// </summary>
        public const string DELETE_USER_CONSUMPTION_ITEM_HISTORY = "/users/{userId}/consumption-history/media-items/{mediaKind}/{mediaItemId}/{libraryId}";

        /// <summary>
        /// The route for clearing the consumption history of the current user.
        /// </summary>
        public const string CLEAR_CONSUMPTION_HISTORY = "/users/me/consumption-history";

        /// <summary>
        /// The route for clearing the consumption history of a user.
        /// </summary>
        public const string CLEAR_USER_CONSUMPTION_HISTORY = "/users/{userId}/consumption-history";

        /// <summary>
        /// The route for clearing the media resume points of the current user.
        /// </summary>
        public const string CLEAR_RESUME_POINTS = "/users/me/media-resume-points";

        /// <summary>
        /// The route for clearing the media resume points of a user.
        /// </summary>
        public const string CLEAR_USER_RESUME_POINTS = "/users/{userId}/media-resume-points";
    }
}
