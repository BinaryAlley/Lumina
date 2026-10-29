namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of routes defined in the Web application.
/// </summary>
public static partial class WebRoutes
{
    /// <summary>
    /// Routes for the media consumption lifecycle actions and resume points the browser clients report to, proxied to the remote API.
    /// </summary>
    public static class MediaConsumption
    {
        public const string START_CONSUMPTION = "{culture}/media-consumption/api-start-consumption/{libraryId}/{mediaKind}/{mediaItemId}";
        public const string REPORT_CONSUMPTION_PROGRESS = "{culture}/media-consumption/api-report-consumption-progress/{recordId}";
        public const string COMPLETE_CONSUMPTION = "{culture}/media-consumption/api-complete-consumption/{recordId}";
        public const string STOP_CONSUMPTION = "{culture}/media-consumption/api-stop-consumption/{recordId}";
        public const string GET_RESUME_POINT = "{culture}/media-consumption/api-get-resume-point/{mediaKind}/{mediaItemId}";
    }
}
