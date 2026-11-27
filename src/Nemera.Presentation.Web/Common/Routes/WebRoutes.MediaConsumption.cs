namespace Nemera.Presentation.Web.Common.Routes;

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
        /// <summary>
        /// The route for the action that starts the consumption of a media item.
        /// </summary>
        public const string START_CONSUMPTION = "{culture}/media-consumption/api-start-consumption/{libraryId}/{mediaKind}/{mediaItemId}";

        /// <summary>
        /// The route for the action that reports the progress of a consumption record.
        /// </summary>
        public const string REPORT_CONSUMPTION_PROGRESS = "{culture}/media-consumption/api-report-consumption-progress/{recordId}";

        /// <summary>
        /// The route for the action that completes a consumption record.
        /// </summary>
        public const string COMPLETE_CONSUMPTION = "{culture}/media-consumption/api-complete-consumption/{recordId}";

        /// <summary>
        /// The route for the action that stops a consumption record.
        /// </summary>
        public const string STOP_CONSUMPTION = "{culture}/media-consumption/api-stop-consumption/{recordId}";

        /// <summary>
        /// The route for the action that retrieves the resume point of a media item of the current user.
        /// </summary>
        public const string GET_RESUME_POINT = "{culture}/media-consumption/api-get-resume-point/{mediaKind}/{mediaItemId}";
    }
}
