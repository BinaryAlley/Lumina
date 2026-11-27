namespace Nemera.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of routes defined in the Web application.
/// </summary>
public static partial class WebRoutes
{
    /// <summary>
    /// Routes for the file system drives endpoints.
    /// </summary>
    public static class Drives
    {
        /// <summary>
        /// The route for the action that retrieves the drives of the host machine.
        /// </summary>
        public const string GET_DRIVES = "drives/api-get-drives";
    }
}
