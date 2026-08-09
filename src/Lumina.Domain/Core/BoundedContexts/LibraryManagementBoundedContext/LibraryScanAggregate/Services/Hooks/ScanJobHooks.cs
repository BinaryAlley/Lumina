namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.Services.Hooks;

/// <summary>
/// Defines the named junction points of the media library scan job graph where plugin jobs can be injected.
/// </summary>
public static class ScanJobHooks
{
    /// <summary>
    /// The hook that runs after the file system discovery job, and before the scan diff job.
    /// </summary>
    public const string AFTER_FILE_SYSTEM_DISCOVERY = "AfterFileSystemDiscovery";

    /// <summary>
    /// The hook that runs after the scan diff job, and before the scan hash job.
    /// </summary>
    public const string AFTER_SCAN_DIFF = "AfterScanDiff";

    /// <summary>
    /// The hook that runs after the scan hash job, and before any metadata enrichment jobs.
    /// </summary>
    public const string BEFORE_METADATA_ENRICHMENT = "BeforeMetadataEnrichment";

    /// <summary>
    /// The hook that runs after any metadata enrichment jobs, and before the scan results save job.
    /// </summary>
    public const string AFTER_METADATA_ENRICHMENT = "AfterMetadataEnrichment";

    /// <summary>
    /// The hook that runs before the scan results save job, which should always be the last job in the directed acyclic job graph.
    /// </summary>
    public const string BEFORE_SCAN_RESULTS_SAVE = "BeforeScanResultsSave";
}
