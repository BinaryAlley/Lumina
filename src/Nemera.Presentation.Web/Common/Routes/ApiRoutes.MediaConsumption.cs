namespace Nemera.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the media consumption lifecycle and resume point endpoints of the remote API.
    /// </summary>
    public static class MediaConsumption
    {
        /// <summary>
        /// The route for the API endpoint that starts the consumption of a media item.
        /// </summary>
        public const string START_CONSUMPTION = "libraries/{libraryId}/media-items/{mediaKind}/{mediaItemId}/consumption-records";

        /// <summary>
        /// The route for the API endpoint that reports the progress of a consumption record.
        /// </summary>
        public const string REPORT_CONSUMPTION_PROGRESS = "users/me/consumption-records/{recordId}/progress";

        /// <summary>
        /// The route for the API endpoint that completes a consumption record.
        /// </summary>
        public const string COMPLETE_CONSUMPTION = "users/me/consumption-records/{recordId}/complete";

        /// <summary>
        /// The route for the API endpoint that stops a consumption record.
        /// </summary>
        public const string STOP_CONSUMPTION = "users/me/consumption-records/{recordId}/stop";

        /// <summary>
        /// The route for the API endpoint that retrieves the resume point of a media item of the current user.
        /// </summary>
        public const string GET_RESUME_POINT = "users/me/media-resume-points/{mediaKind}/{mediaItemId}";
    }
}
