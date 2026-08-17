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
        public const string INDEX = "/";
        public const string INDEX_CULTURED = "{culture}";
        public const string PRIVACY = "{culture}/privacy";
        public const string ERROR = "{culture}/error";
        public const string NOT_FOUND = "{culture}/not-found";
    }
}
