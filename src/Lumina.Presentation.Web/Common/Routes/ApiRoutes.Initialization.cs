namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the initialization endpoint of the remote API.
    /// </summary>
    public static class Initialization
    {
        public const string SETUP_APPLICATION = "initialization";
        public const string CHECK_INITIALIZATION = "initialization";
    }
}
