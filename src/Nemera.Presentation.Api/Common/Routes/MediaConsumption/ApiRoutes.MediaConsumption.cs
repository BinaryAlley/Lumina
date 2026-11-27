namespace Lumina.Presentation.Api.Common.Routes.MediaConsumption;

/// <summary>
/// Class for the collection of the media consumption routes of the API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the media consumption lifecycle, records, and resume points of the current user.
    /// </summary>
    public static class MediaConsumption
    {
        /// <summary>
        /// The route for starting the consumption of a media item of a library.
        /// </summary>
        public const string START_CONSUMPTION = "/libraries/{libraryId}/media-items/{mediaKind}/{mediaItemId}/consumption-records";

        /// <summary>
        /// The route for getting a consumption record of the current user.
        /// </summary>
        public const string GET_CONSUMPTION_RECORD = "/users/me/consumption-records/{recordId}";

        /// <summary>
        /// The route for reporting the progress of a consumption record of the current user.
        /// </summary>
        public const string REPORT_CONSUMPTION_PROGRESS = "/users/me/consumption-records/{recordId}/progress";

        /// <summary>
        /// The route for completing a consumption record of the current user.
        /// </summary>
        public const string COMPLETE_CONSUMPTION = "/users/me/consumption-records/{recordId}/complete";

        /// <summary>
        /// The route for stopping a consumption record of the current user.
        /// </summary>
        public const string STOP_CONSUMPTION = "/users/me/consumption-records/{recordId}/stop";

        /// <summary>
        /// The route for getting the resume point of a media item of the current user.
        /// </summary>
        public const string GET_RESUME_POINT = "/users/me/media-resume-points/{mediaKind}/{mediaItemId}";
    }
}
