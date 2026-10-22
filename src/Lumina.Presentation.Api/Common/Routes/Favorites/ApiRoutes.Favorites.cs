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
        public const string GET_FAVORITES = "/users/me/favorites";
        public const string ADD_FAVORITE = "/users/me/favorites";
        public const string REMOVE_FAVORITE = "/users/me/favorites/{mediaKind}/{mediaItemId}";
        public const string GET_FAVORITE_STATUS = "/users/me/favorites/status";
    }
}
