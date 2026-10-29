#region ========================================================================= USING =====================================================================================
using System.Diagnostics;
#endregion

namespace Lumina.Infrastructure.Common.Models.DTO.Configuration;

/// <summary>
/// Data transfer object for deserializing the settings of the audio normalization pipeline.
/// </summary>
[DebuggerDisplay("Section name: {SECTION_NAME}")]
public sealed class TranscodingSettingsDto
{
    public const string SECTION_NAME = "TranscodingSettings";

    /// <summary>
    /// Gets or sets the file name or path of the ffmpeg binary, relative to the process search path when not rooted.
    /// </summary>
    public string FfmpegPath { get; set; } = "ffmpeg";

    /// <summary>
    /// Gets or sets the maximum number of normalization jobs that may run at the same time.
    /// </summary>
    public int MaxConcurrentJobs { get; set; } = 2;

    /// <summary>
    /// Gets or sets the maximum number of normalization jobs a single user may have running at the same time. A production that was
    /// started by another user can still be waited for, because it is shared by every request for the same track and delivery.
    /// </summary>
    public int MaxConcurrentJobsPerUser { get; set; } = 1;

    /// <summary>
    /// Gets or sets the maximum total size of the normalized audio cache, in bytes.
    /// </summary>
    public long MaxCacheBytes { get; set; } = 2L * 1024 * 1024 * 1024;

    /// <summary>
    /// Gets or sets the maximum duration of a single normalization job, in seconds, after which the job is stopped.
    /// </summary>
    public int JobTimeoutSeconds { get; set; } = 900;

    /// <summary>
    /// Gets or sets the period, in seconds, during which a cache entry that was just produced or served is protected from eviction.
    /// </summary>
    public int EvictionGraceSeconds { get; set; } = 120;

    /// <summary>
    /// Gets or sets the directory where normalized audio is cached, relative to the API base directory when not rooted. When absent or empty, a
    /// private directory under the system temporary directory is used.
    /// </summary>
    public string? CacheDirectory { get; set; }
}
