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
        public const string GET_FAVORITES = "users/me/favorites";
        public const string ADD_FAVORITE = "users/me/favorites";
        public const string REMOVE_FAVORITE = "users/me/favorites/{mediaKind}/{mediaItemId}";
        public const string GET_FAVORITE_STATUS = "users/me/favorites/status";
    }
}
