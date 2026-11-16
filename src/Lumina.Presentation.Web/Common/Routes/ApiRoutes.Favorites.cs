namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the favorites endpoints of the remote API.
    /// </summary>
    public static class Favorites
    {
        /// <summary>
        /// The route for the API endpoint that retrieves the favorites of the current user.
        /// </summary>
        public const string GET_FAVORITES = "users/me/favorites";

        /// <summary>
        /// The route for the API endpoint that adds a media item to the favorites of the current user.
        /// </summary>
        public const string ADD_FAVORITE = "users/me/favorites";

        /// <summary>
        /// The route for the API endpoint that removes a media item from the favorites of the current user.
        /// </summary>
        public const string REMOVE_FAVORITE = "users/me/favorites/{mediaKind}/{mediaItemId}";

        /// <summary>
        /// The route for the API endpoint that retrieves the favorite status of a media item for the current user.
        /// </summary>
        public const string GET_FAVORITE_STATUS = "users/me/favorites/status";
    }
}
