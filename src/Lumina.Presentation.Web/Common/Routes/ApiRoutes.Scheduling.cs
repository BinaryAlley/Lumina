namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of remote API routes called by this Web application.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the scheduled job endpoints of the remote API.
    /// </summary>
    public static class ScheduledJobs
    {
        public const string GET_SCHEDULED_JOBS = "scheduled-jobs";
        public const string ADD_SCHEDULED_JOB = "scheduled-jobs";
        public const string GET_SCHEDULED_JOB_HISTORY = "scheduled-jobs/history";
        public const string REMOVE_SCHEDULED_JOB = "scheduled-jobs/{scheduledJobId}";
        public const string START_SCHEDULED_JOB = "scheduled-jobs/{scheduledJobId}/start";
        public const string STOP_SCHEDULED_JOB = "scheduled-jobs/{scheduledJobId}/stop";
        public const string FIRE_SCHEDULED_JOB = "scheduled-jobs/{scheduledJobId}/fire";
        public const string GET_DISPLAY_PREFERENCES = "scheduled-jobs/display-preferences";
        public const string UPDATE_DISPLAY_PREFERENCES = "scheduled-jobs/display-preferences";
    }
}
