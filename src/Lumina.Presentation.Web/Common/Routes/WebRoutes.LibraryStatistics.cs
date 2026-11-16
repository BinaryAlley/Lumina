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
        /// <summary>
        /// The route for the library statistics page.
        /// </summary>
        public const string INDEX = "{culture}/library/statistics";

        /// <summary>
        /// The route for the action that retrieves the consumption statistics of the selected user.
        /// </summary>
        public const string GET_STATISTICS = "{culture}/library/statistics/api-get-statistics";

        /// <summary>
        /// The route for the action that retrieves the consumption statistics of all users.
        /// </summary>
        public const string GET_ALL_USERS_STATISTICS = "{culture}/library/statistics/api-get-all-users-statistics";

        /// <summary>
        /// The route for the action that retrieves the recently consumed media items of the selected user.
        /// </summary>
        public const string GET_RECENTLY_CONSUMED = "{culture}/library/statistics/api-get-recently-consumed";

        /// <summary>
        /// The route for the action that retrieves the most consumed media items of the selected user.
        /// </summary>
        public const string GET_MOST_CONSUMED = "{culture}/library/statistics/api-get-most-consumed";

        /// <summary>
        /// The route for the action that retrieves the tag consumption statistics of the selected user.
        /// </summary>
        public const string GET_TAG_CONSUMPTION = "{culture}/library/statistics/api-get-tag-consumption";

        /// <summary>
        /// The route for the action that retrieves the genre consumption statistics of the selected user.
        /// </summary>
        public const string GET_GENRE_CONSUMPTION = "{culture}/library/statistics/api-get-genre-consumption";

        /// <summary>
        /// The route for the action that retrieves the enabled media libraries available for the statistics filter.
        /// </summary>
        public const string GET_LIBRARIES = "{culture}/library/statistics/api-get-libraries";

        /// <summary>
        /// The route for the action that retrieves the users available for the statistics user selector.
        /// </summary>
        public const string GET_USERS = "{culture}/library/statistics/api-get-users";

        /// <summary>
        /// The route for the action that deletes a media item from the consumption history.
        /// </summary>
        public const string DELETE_CONSUMPTION_ITEM_HISTORY = "{culture}/library/statistics/api-delete-consumption-item-history";

        /// <summary>
        /// The route for the action that clears the consumption history.
        /// </summary>
        public const string CLEAR_CONSUMPTION_HISTORY = "{culture}/library/statistics/api-clear-consumption-history";

        /// <summary>
        /// The route for the action that clears the media resume points.
        /// </summary>
        public const string CLEAR_RESUME_POINTS = "{culture}/library/statistics/api-clear-resume-points";
    }
}
