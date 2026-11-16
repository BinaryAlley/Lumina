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
        /// <summary>
        /// The route for the API endpoint that retrieves the scheduled jobs.
        /// </summary>
        public const string GET_SCHEDULED_JOBS = "scheduled-jobs";

        /// <summary>
        /// The route for the API endpoint that adds a scheduled job.
        /// </summary>
        public const string ADD_SCHEDULED_JOB = "scheduled-jobs";

        /// <summary>
        /// The route for the API endpoint that retrieves the execution history of the scheduled jobs.
        /// </summary>
        public const string GET_SCHEDULED_JOB_HISTORY = "scheduled-jobs/history";

        /// <summary>
        /// The route for the API endpoint that removes a scheduled job.
        /// </summary>
        public const string REMOVE_SCHEDULED_JOB = "scheduled-jobs/{scheduledJobId}";

        /// <summary>
        /// The route for the API endpoint that starts a scheduled job.
        /// </summary>
        public const string START_SCHEDULED_JOB = "scheduled-jobs/{scheduledJobId}/start";

        /// <summary>
        /// The route for the API endpoint that stops a scheduled job.
        /// </summary>
        public const string STOP_SCHEDULED_JOB = "scheduled-jobs/{scheduledJobId}/stop";

        /// <summary>
        /// The route for the API endpoint that fires a scheduled job immediately.
        /// </summary>
        public const string FIRE_SCHEDULED_JOB = "scheduled-jobs/{scheduledJobId}/fire";

        /// <summary>
        /// The route for the API endpoint that retrieves the display preferences of the scheduled jobs.
        /// </summary>
        public const string GET_DISPLAY_PREFERENCES = "scheduled-jobs/display-preferences";

        /// <summary>
        /// The route for the API endpoint that updates the display preferences of the scheduled jobs.
        /// </summary>
        public const string UPDATE_DISPLAY_PREFERENCES = "scheduled-jobs/display-preferences";
    }
}
