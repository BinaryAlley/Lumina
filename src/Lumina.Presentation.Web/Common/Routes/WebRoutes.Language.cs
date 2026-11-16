namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of routes defined in the Web application.
/// </summary>
public static partial class WebRoutes
{
    /// <summary>
    /// Routes for the language tool pages.
    /// </summary>
    public static class Language
    {
        /// <summary>
        /// The route for the action that sets the display language of the Web application.
        /// </summary>
        public const string SET_LANGUAGE = "{culture}/tools/language/set-language";
    }
}
