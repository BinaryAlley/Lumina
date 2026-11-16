namespace Lumina.Presentation.Web.Common.Routes;

/// <summary>
/// Class for the collection of routes defined in the Web application.
/// </summary>
public static partial class WebRoutes
{
    /// <summary>
    /// Routes for the scheduled jobs pages.
    /// </summary>
    public static class Scheduler
    {
        /// <summary>
        /// The route for the scheduled jobs page.
        /// </summary>
        public const string INDEX = "{culture}/admin/scheduled-jobs";

        /// <summary>
        /// The route for the action that retrieves the scheduled jobs.
        /// </summary>
        public const string GET_SCHEDULED_JOBS = "{culture}/admin/api-scheduled-jobs";

        /// <summary>
        /// The route for the action that adds a scheduled job.
        /// </summary>
        public const string ADD_SCHEDULED_JOB = "{culture}/admin/api-scheduled-jobs/add";

        /// <summary>
        /// The route for the action that retrieves the execution history of the scheduled jobs.
        /// </summary>
        public const string GET_SCHEDULED_JOB_HISTORY = "{culture}/admin/api-scheduled-jobs/history";

        /// <summary>
        /// The route for the action that removes a scheduled job.
        /// </summary>
        public const string REMOVE_SCHEDULED_JOB = "{culture}/admin/api-scheduled-jobs/{scheduledJobId}";

        /// <summary>
        /// The route for the action that starts a scheduled job.
        /// </summary>
        public const string START_SCHEDULED_JOB = "{culture}/admin/api-scheduled-jobs/{scheduledJobId}/start";

        /// <summary>
        /// The route for the action that stops a scheduled job.
        /// </summary>
        public const string STOP_SCHEDULED_JOB = "{culture}/admin/api-scheduled-jobs/{scheduledJobId}/stop";

        /// <summary>
        /// The route for the action that fires a scheduled job immediately.
        /// </summary>
        public const string FIRE_SCHEDULED_JOB = "{culture}/admin/api-scheduled-jobs/{scheduledJobId}/fire";

        /// <summary>
        /// The route for the action that retrieves the display preferences of the scheduled jobs.
        /// </summary>
        public const string GET_DISPLAY_PREFERENCES = "{culture}/admin/api-scheduled-jobs/display-preferences";

        /// <summary>
        /// The route for the action that updates the display preferences of the scheduled jobs.
        /// </summary>
        public const string UPDATE_DISPLAY_PREFERENCES = "{culture}/admin/api-scheduled-jobs/display-preferences";
    }
}
