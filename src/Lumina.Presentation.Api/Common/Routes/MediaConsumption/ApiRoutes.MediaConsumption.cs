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
        public const string START_CONSUMPTION = "/libraries/{libraryId}/media-items/{mediaKind}/{mediaItemId}/consumption-records";
        public const string GET_CONSUMPTION_RECORD = "/users/me/consumption-records/{recordId}";
        public const string REPORT_CONSUMPTION_PROGRESS = "/users/me/consumption-records/{recordId}/progress";
        public const string COMPLETE_CONSUMPTION = "/users/me/consumption-records/{recordId}/complete";
        public const string STOP_CONSUMPTION = "/users/me/consumption-records/{recordId}/stop";
        public const string GET_RESUME_POINT = "/users/me/media-resume-points/{mediaKind}/{mediaItemId}";
    }
}
