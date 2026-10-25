namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the media consumption lifecycle endpoints of the remote API.
    /// </summary>
    public static class MediaConsumption
    {
        public const string START_CONSUMPTION = "libraries/{libraryId}/media-items/{mediaKind}/{mediaItemId}/consumption-records";
        public const string REPORT_CONSUMPTION_PROGRESS = "users/me/consumption-records/{recordId}/progress";
        public const string COMPLETE_CONSUMPTION = "users/me/consumption-records/{recordId}/complete";
        public const string STOP_CONSUMPTION = "users/me/consumption-records/{recordId}/stop";
    }
}
