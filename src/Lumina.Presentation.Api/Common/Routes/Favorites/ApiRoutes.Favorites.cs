namespace Lumina.Presentation.Api.Common.Routes.Favorites;

/// <summary>
/// Class for the collection of the favorite routes of the API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the favorites of the current user.
    /// </summary>
    public static class Favorites
    {
        /// <summary>
        /// The route for getting the favorites of the current user.
        /// </summary>
        public const string GET_FAVORITES = "/users/me/favorites";

        /// <summary>
        /// The route for adding a media item to the favorites of the current user.
        /// </summary>
        public const string ADD_FAVORITE = "/users/me/favorites";

        /// <summary>
        /// The route for removing a media item from the favorites of the current user.
        /// </summary>
        public const string REMOVE_FAVORITE = "/users/me/favorites/{mediaKind}/{mediaItemId}";

        /// <summary>
        /// The route for getting the favorite status of a media item of the current user.
        /// </summary>
        public const string GET_FAVORITE_STATUS = "/users/me/favorites/status";
    }
}
