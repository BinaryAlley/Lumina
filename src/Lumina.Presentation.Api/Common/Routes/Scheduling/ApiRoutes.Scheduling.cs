namespace Lumina.Presentation.Api.Common.Routes.Scheduling;

/// <summary>
/// Class for the collection of the scheduled job routes of the API.
/// </summary>
public static partial class ApiRoutes
{
    public static class ScheduledJobs
    {
        public const string GET_SCHEDULED_JOBS = "/scheduled-jobs";
        public const string ADD_SCHEDULED_JOB = "/scheduled-jobs";
        public const string GET_SCHEDULED_JOB_HISTORY = "/scheduled-jobs/history";
        public const string REMOVE_SCHEDULED_JOB = "/scheduled-jobs/{scheduledJobId}";
        public const string START_SCHEDULED_JOB = "/scheduled-jobs/{scheduledJobId}/start";
        public const string STOP_SCHEDULED_JOB = "/scheduled-jobs/{scheduledJobId}/stop";
        public const string FIRE_SCHEDULED_JOB = "/scheduled-jobs/{scheduledJobId}/fire";
    }
}
