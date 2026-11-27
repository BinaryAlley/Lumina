namespace Lumina.Presentation.Api.Common.Routes.Scheduling;

/// <summary>
/// Class for the collection of the scheduled job routes of the API.
/// </summary>
public static partial class ApiRoutes
{
    /// <summary>
    /// Routes for the scheduled jobs of the application.
    /// </summary>
    public static class ScheduledJobs
    {
        /// <summary>
        /// The route for getting the scheduled jobs.
        /// </summary>
        public const string GET_SCHEDULED_JOBS = "/scheduled-jobs";

        /// <summary>
        /// The route for adding a scheduled job.
        /// </summary>
        public const string ADD_SCHEDULED_JOB = "/scheduled-jobs";

        /// <summary>
        /// The route for getting the execution history of the scheduled jobs.
        /// </summary>
        public const string GET_SCHEDULED_JOB_HISTORY = "/scheduled-jobs/history";

        /// <summary>
        /// The route for removing a scheduled job.
        /// </summary>
        public const string REMOVE_SCHEDULED_JOB = "/scheduled-jobs/{scheduledJobId}";

        /// <summary>
        /// The route for starting a scheduled job.
        /// </summary>
        public const string START_SCHEDULED_JOB = "/scheduled-jobs/{scheduledJobId}/start";

        /// <summary>
        /// The route for stopping a scheduled job.
        /// </summary>
        public const string STOP_SCHEDULED_JOB = "/scheduled-jobs/{scheduledJobId}/stop";

        /// <summary>
        /// The route for firing a scheduled job immediately.
        /// </summary>
        public const string FIRE_SCHEDULED_JOB = "/scheduled-jobs/{scheduledJobId}/fire";

        /// <summary>
        /// The route for getting the display preferences of the scheduler.
        /// </summary>
        public const string GET_DISPLAY_PREFERENCES = "/scheduled-jobs/display-preferences";

        /// <summary>
        /// The route for updating the display preferences of the scheduler.
        /// </summary>
        public const string UPDATE_DISPLAY_PREFERENCES = "/scheduled-jobs/display-preferences";
    }
}
