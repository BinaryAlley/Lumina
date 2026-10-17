#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Application.Common.Infrastructure.Validation;
using Lumina.Application.Common.Utilities;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Infrastructure.Common.Caching;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.Common.Validators;

/// <summary>
/// Validates the needed validation rules for the TranscodingSettings application configuration settings section.
/// </summary>
public class TranscodingSettingsDtoValidator : AbstractValidator<TranscodingSettingsDto>
{
    private const int MAX_CONCURRENT_JOBS = 64;
    private const int MAX_JOB_TIMEOUT_SECONDS = 86400;
    private const int MAX_EVICTION_GRACE_SECONDS = 86400;

    private static readonly StringComparison s_pathComparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    private readonly IPathService _pathService;
    private readonly string _mediaRootDirectory;
    private readonly string[] _conflictingDirectories;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranscodingSettingsDtoValidator"/> class.
    /// </summary>
    /// <param name="mediaSettings">The media settings, used to reject a cache directory that would be served by the media route.</param>
    /// <param name="pluginsSettings">The plugin settings, used to reject a cache directory that would overlap the plugins directory.</param>
    /// <param name="pathService">The service used to check whether two directories overlap.</param>
    public TranscodingSettingsDtoValidator(IOptions<MediaSettingsDto> mediaSettings, IOptions<PluginsSettingsDto> pluginsSettings, IPathService pathService)
    {
        _pathService = pathService;
        _mediaRootDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, mediaSettings.Value.RootDirectory));
        _conflictingDirectories = BuildConflictingDirectories(pluginsSettings.Value);

        RuleFor(settings => settings.FfmpegPath)
            .NotEmpty()
            .WithError(Errors.Errors.Configuration.TranscodingFfmpegPathCannotBeEmpty);

        RuleFor(settings => settings.MaxConcurrentJobs)
            .GreaterThan(0)
            .WithError(Errors.Errors.Configuration.TranscodingMaxConcurrentJobsMustBePositive);

        RuleFor(settings => settings.MaxConcurrentJobs)
            .Must(maxConcurrentJobs => maxConcurrentJobs <= MAX_CONCURRENT_JOBS)
            .WithError(Errors.Errors.Configuration.TranscodingMaxConcurrentJobsIsTooLarge);

        RuleFor(settings => settings.MaxCacheBytes)
            .GreaterThan(0)
            .WithError(Errors.Errors.Configuration.TranscodingMaxCacheBytesMustBePositive);

        RuleFor(settings => settings.JobTimeoutSeconds)
            .GreaterThan(0)
            .WithError(Errors.Errors.Configuration.TranscodingJobTimeoutMustBePositive);

        // CancellationTokenSource(TimeSpan) cannot represent an arbitrarily large delay, and a job that may run for days is a configuration error.
        RuleFor(settings => settings.JobTimeoutSeconds)
            .Must(jobTimeoutSeconds => jobTimeoutSeconds <= MAX_JOB_TIMEOUT_SECONDS)
            .WithError(Errors.Errors.Configuration.TranscodingJobTimeoutIsTooLong);

        RuleFor(settings => settings.EvictionGraceSeconds)
            .GreaterThan(0)
            .WithError(Errors.Errors.Configuration.TranscodingEvictionGraceSecondsMustBePositive);

        RuleFor(settings => settings.EvictionGraceSeconds)
            .Must(evictionGraceSeconds => evictionGraceSeconds <= MAX_EVICTION_GRACE_SECONDS)
            .WithError(Errors.Errors.Configuration.TranscodingEvictionGraceSecondsIsTooLong);

        RuleFor(settings => settings.CacheDirectory)
            .Must(BeAValidDirectoryPath)
            .WithError(Errors.Errors.Configuration.TranscodingCacheDirectoryIsInvalid)
            .When(settings => !string.IsNullOrWhiteSpace(settings.CacheDirectory));

        // A malformed path is reported by the rule above, so the rules below must not try to resolve it: Path.GetFullPath would throw and abort
        // startup with a raw exception instead of a configuration error.
        RuleFor(settings => settings.CacheDirectory)
            .Must(BeConfiguredDirectoryOutsideMediaRoot)
            .WithError(Errors.Errors.Configuration.TranscodingCacheDirectoryIsInsideMediaRoot)
            .When(settings => !string.IsNullOrWhiteSpace(settings.CacheDirectory) && BeAValidDirectoryPath(settings.CacheDirectory));

        // The reverse containment is just as destructive: a cache directory that contains the media root would delete the media when it is cleaned.
        RuleFor(settings => settings.CacheDirectory)
            .Must(BeConfiguredDirectoryNotContainingMediaRoot)
            .WithError(Errors.Errors.Configuration.TranscodingCacheDirectoryContainsMediaRoot)
            .When(settings => !string.IsNullOrWhiteSpace(settings.CacheDirectory) && BeAValidDirectoryPath(settings.CacheDirectory));

        // The cache directory is cleaned at startup, so it must never resolve to a location whose cleanup would damage the application or the host.
        RuleFor(settings => settings.CacheDirectory)
            .Must(BeInSafeLocation)
            .WithError(Errors.Errors.Configuration.TranscodingCacheDirectoryIsUnsafeLocation)
            .When(settings => !string.IsNullOrWhiteSpace(settings.CacheDirectory) && BeAValidDirectoryPath(settings.CacheDirectory));

        // When no directory is configured the service uses a per-user directory under the system temporary directory. A failure of that default is a
        // different configuration problem from a CacheDirectory the operator chose, so it is reported with its own error and never names CacheDirectory.
        RuleFor(settings => settings.CacheDirectory)
            .Must(_ => BeDefaultDirectoryOutsideMediaRoot())
            .WithError(Errors.Errors.Configuration.TranscodingDefaultCacheDirectoryIsInsideMediaRoot)
            .When(settings => string.IsNullOrWhiteSpace(settings.CacheDirectory));
    }

    /// <summary>
    /// Checks whether the provided cache directory is a path the file system accepts.
    /// </summary>
    /// <param name="cacheDirectory">The configured cache directory, or <see langword="null"/> when the default is used.</param>
    /// <returns><see langword="true"/> when the path is valid, otherwise <see langword="false"/>.</returns>
    private static bool BeAValidDirectoryPath(string? cacheDirectory)
    {
        if (string.IsNullOrWhiteSpace(cacheDirectory))
            return true;
        try
        {
            Path.GetFullPath(cacheDirectory);
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks whether the configured cache directory is outside the media root, which is served without authentication at the media route.
    /// </summary>
    /// <param name="cacheDirectory">The configured cache directory. It is already known to be a valid path.</param>
    /// <returns><see langword="true"/> when the cache is outside the media root, otherwise <see langword="false"/>.</returns>
    private bool BeConfiguredDirectoryOutsideMediaRoot(string? cacheDirectory)
    {
        try
        {
            return !IsSameOrWithin(TranscodingCachePaths.GetRootDirectory(cacheDirectory), _mediaRootDirectory);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            // The validity rule owns the malformed path error; this rule never turns it into an unhandled startup exception.
            return true;
        }
    }

    /// <summary>
    /// Checks whether the configured cache directory does not contain the media root.
    /// </summary>
    /// <param name="cacheDirectory">The configured cache directory. It is already known to be a valid path.</param>
    /// <returns><see langword="true"/> when the cache does not contain the media root, otherwise <see langword="false"/>.</returns>
    private bool BeConfiguredDirectoryNotContainingMediaRoot(string? cacheDirectory)
    {
        try
        {
            return !IsSameOrWithin(_mediaRootDirectory, TranscodingCachePaths.GetRootDirectory(cacheDirectory));
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            // The validity rule owns the malformed path error; this rule never turns it into an unhandled startup exception.
            return true;
        }
    }

    /// <summary>
    /// Checks whether the configured cache directory is a location whose cleanup cannot damage the application or the host.
    /// </summary>
    /// <param name="cacheDirectory">The configured cache directory. It is already known to be a valid path.</param>
    /// <returns><see langword="true"/> when the location is safe, otherwise <see langword="false"/>.</returns>
    private bool BeInSafeLocation(string? cacheDirectory)
    {
        try
        {
            string resolvedCacheDirectory = TranscodingCachePaths.GetRootDirectory(cacheDirectory);
            if (IsFileSystemRoot(resolvedCacheDirectory))
                return false;
            // The cache must never be the application base directory or one of its ancestors, because cleaning it would remove the application itself.
            if (IsSameOrWithin(AppContext.BaseDirectory, resolvedCacheDirectory))
                return false;
            // The logs and plugins directories are managed by the application, so the cache must not overlap them in either direction.
            foreach (string conflictingDirectory in _conflictingDirectories)
                if (IsSameOrWithin(resolvedCacheDirectory, conflictingDirectory) || IsSameOrWithin(conflictingDirectory, resolvedCacheDirectory))
                    return false;
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            // The validity rule owns the malformed path error; this rule never turns it into an unhandled startup exception.
            return true;
        }
    }

    /// <summary>
    /// Checks whether the default cache directory, used when none is configured, is outside the media root.
    /// </summary>
    /// <returns><see langword="true"/> when the default cache is outside the media root, otherwise <see langword="false"/>.</returns>
    private bool BeDefaultDirectoryOutsideMediaRoot()
    {
        try
        {
            return !IsSameOrWithin(TranscodingCachePaths.GetRootDirectory(configuredDirectory: null), _mediaRootDirectory);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            // A temporary directory that cannot be resolved is reported by the startup initializer; it is not a media root containment error.
            return true;
        }
    }

    /// <summary>
    /// Checks whether a path is equal to, or is located inside, another path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <param name="parentPath">The path that must contain, or be equal to, the checked path.</param>
    /// <returns><see langword="true"/> when the path is equal to or inside the parent path, otherwise <see langword="false"/>.</returns>
    private bool IsSameOrWithin(string path, string parentPath)
    {
        string normalizedPath = NormalizePath(path);
        string normalizedParentPath = NormalizePath(parentPath);
        if (string.Equals(normalizedPath, normalizedParentPath, s_pathComparison))
            return true;
        return _pathService.IsPathWithin(normalizedPath, normalizedParentPath);
    }

    /// <summary>
    /// Checks whether a path is the root of a file system, or of a volume, instead of a directory inside it.
    /// </summary>
    /// <param name="path">The absolute path to check.</param>
    /// <returns><see langword="true"/> when the path is a file system root, otherwise <see langword="false"/>.</returns>
    private static bool IsFileSystemRoot(string path)
    {
        string normalizedPath = NormalizePath(path);
        string? rootPath = Path.GetPathRoot(normalizedPath);
        return rootPath is not null && string.Equals(normalizedPath, rootPath, s_pathComparison);
    }

    /// <summary>
    /// Gets the full form of a path, without a trailing separator unless the path itself is the root of a file system or volume.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>The normalized absolute path.</returns>
    private static string NormalizePath(string path)
    {
        string fullPath = Path.GetFullPath(path);
        string? rootPath = Path.GetPathRoot(fullPath);
        // The root must keep its trailing separator, otherwise a drive root such as "C:\" degenerates into the drive relative path "C:".
        if (rootPath is null || fullPath.Length <= rootPath.Length)
            return fullPath;
        return fullPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    /// <summary>
    /// Gets the directories that the cache directory must never overlap: the logs directory and the plugins directory.
    /// </summary>
    /// <param name="pluginsSettings">The plugin settings, whose directory is resolved against the application base directory.</param>
    /// <returns>The absolute paths of the directories the cache must not overlap.</returns>
    private static string[] BuildConflictingDirectories(PluginsSettingsDto pluginsSettings)
    {
        List<string> conflictingDirectories = [];
        if (!string.IsNullOrWhiteSpace(pluginsSettings.Directory))
            AddConflictingDirectory(conflictingDirectories, Path.Combine(AppContext.BaseDirectory, pluginsSettings.Directory));
        // The local log path is always the logs directory of the application base directory; the environment variable and the container path cover
        // the other two ways the log path is selected at startup.
        AddConflictingDirectory(conflictingDirectories, Path.Combine(AppContext.BaseDirectory, "logs"));
        AddConflictingDirectory(conflictingDirectories, Environment.GetEnvironmentVariable("LOG_PATH"));
        if (string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase))
            AddConflictingDirectory(conflictingDirectories, "/logs");
        return [.. conflictingDirectories];
    }

    /// <summary>
    /// Adds the full form of a directory to the list of conflicting directories, when it can be resolved and is not already listed.
    /// </summary>
    /// <param name="conflictingDirectories">The list of conflicting directories to add to.</param>
    /// <param name="directory">The directory to add, or <see langword="null"/> when it is not configured.</param>
    private static void AddConflictingDirectory(List<string> conflictingDirectories, string? directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
            return;
        try
        {
            string resolvedDirectory = NormalizePath(directory);
            if (!conflictingDirectories.Contains(resolvedDirectory, StringComparer.FromComparison(s_pathComparison)))
                conflictingDirectories.Add(resolvedDirectory);
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            // A malformed log path is reported by the logging configuration, not by the cache validation.
        }
    }
}
