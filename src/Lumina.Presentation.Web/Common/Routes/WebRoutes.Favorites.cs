namespace Lumina.Presentation.Web.Common.Routes;

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
        public const string INDEX = "{culture}/favorites";
        public const string GET_FAVORITES = "{culture}/favorites/api-get-favorites";
        public const string GET_FAVORITE_STATUS = "{culture}/favorites/api-get-favorite-status";
        public const string ADD_FAVORITE = "{culture}/favorites/api-add-favorite";
        public const string REMOVE_FAVORITE = "{culture}/favorites/{mediaKind}/{mediaItemId}/api-remove-favorite";
    }
}
