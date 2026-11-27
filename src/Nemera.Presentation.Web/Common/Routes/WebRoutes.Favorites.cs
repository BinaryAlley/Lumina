namespace Nemera.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of routes defined in the Web application.
/// </summary>
public static partial class WebRoutes
{
    /// <summary>
    /// Routes for the favorites pages.
    /// </summary>
    public static class Favorites
    {
        /// <summary>
        /// The route for the favorites page.
        /// </summary>
        public const string INDEX = "{culture}/favorites";

        /// <summary>
        /// The route for the action that retrieves the favorites of the current user.
        /// </summary>
        public const string GET_FAVORITES = "{culture}/favorites/api-get-favorites";

        /// <summary>
        /// The route for the action that retrieves the favorite status of a media item for the current user.
        /// </summary>
        public const string GET_FAVORITE_STATUS = "{culture}/favorites/api-get-favorite-status";

        /// <summary>
        /// The route for the action that adds a media item to the favorites of the current user.
        /// </summary>
        public const string ADD_FAVORITE = "{culture}/favorites/api-add-favorite";

        /// <summary>
        /// The route for the action that removes a media item from the favorites of the current user.
        /// </summary>
        public const string REMOVE_FAVORITE = "{culture}/favorites/{mediaKind}/{mediaItemId}/api-remove-favorite";
    }
}
