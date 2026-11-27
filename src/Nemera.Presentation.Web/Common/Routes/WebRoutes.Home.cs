namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of routes defined in the Web application.
/// </summary>
public static partial class WebRoutes
{
    /// <summary>
    /// Routes for the home pages.
    /// </summary>
    public static class Home
    {
        /// <summary>
        /// The route for the home page at the root of the application.
        /// </summary>
        public const string INDEX = "/";

        /// <summary>
        /// The route for the culture-specific home page.
        /// </summary>
        public const string INDEX_CULTURED = "{culture}";

        /// <summary>
        /// The route for the privacy policy page.
        /// </summary>
        public const string PRIVACY = "{culture}/privacy";

        /// <summary>
        /// The route for the error page.
        /// </summary>
        public const string ERROR = "{culture}/error";

        /// <summary>
        /// The route for the not found page.
        /// </summary>
        public const string NOT_FOUND = "{culture}/not-found";
    }
}
