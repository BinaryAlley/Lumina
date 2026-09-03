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
        public const string INDEX = "{culture}/admin/scheduled-jobs";
        public const string GET_SCHEDULED_JOBS = "{culture}/admin/api-scheduled-jobs";
        public const string ADD_SCHEDULED_JOB = "{culture}/admin/api-scheduled-jobs/add";
        public const string GET_SCHEDULED_JOB_HISTORY = "{culture}/admin/api-scheduled-jobs/history";
        public const string REMOVE_SCHEDULED_JOB = "{culture}/admin/api-scheduled-jobs/{scheduledJobId}";
        public const string START_SCHEDULED_JOB = "{culture}/admin/api-scheduled-jobs/{scheduledJobId}/start";
        public const string STOP_SCHEDULED_JOB = "{culture}/admin/api-scheduled-jobs/{scheduledJobId}/stop";
        public const string FIRE_SCHEDULED_JOB = "{culture}/admin/api-scheduled-jobs/{scheduledJobId}/fire";
    }
}
