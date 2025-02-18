#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics;
#endregion

namespace Lumina.Contracts.Responses.MediaLibrary.Management;

/// <summary>
/// Represents a media library scan progress response.
/// </summary>
/// <param name="TotalJobs">The total number of jobs to be processed by the scan.</param>
/// <param name="CompletedJobs">The number of jobs that have been processed.</param>
/// <param name="CurrentJobProgress">The progress of the currently processing job.</param>
/// <param name="Status">The status of the scan.</param>
/// <param name="OverallProgressPercentage">The ratio between the number of processed jobs and the total number of jobs to process, as percentage.</param>
[DebuggerDisplay("TotalJobs: {TotalJobs} CompletedJobs: {CompletedJobs}")]
public record MediaLibraryScanProgressResponse(
     int TotalJobs,
     int CompletedJobs,
     MediaLibraryScanJobProgressResponse? CurrentJobProgress,
     LibraryScanJobStatus Status,
     decimal OverallProgressPercentage
);
