#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Application.Fixtures.Common.Infrastructure.Models.DTO.Configuration;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Services;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Path;
using Lumina.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.Strategies.Platform;
using Lumina.Infrastructure.Common.Errors;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
using Lumina.Infrastructure.Common.Validators;
using Lumina.Infrastructure.Fixtures.Common.Models.DTO.Configuration;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
#endregion

namespace Lumina.Infrastructure.UnitTests.Common.Validators;

/// <summary>
/// Contains unit tests for the <see cref="TranscodingSettingsDtoValidator"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TranscodingSettingsDtoValidatorTests : IDisposable
{
    private readonly TranscodingSettingsDtoValidator _validator;
    private readonly TranscodingSettingsDtoFixture _transcodingSettingsDtoFixture = new();
    private readonly MediaSettingsDtoFixture _mediaSettingsDtoFixture = new();
    private readonly PluginsSettingsDtoFixture _pluginsSettingsDtoFixture = new();
    private readonly string _sandboxRoot;
    private readonly string _mediaRoot;
    private readonly string _validCacheDirectory;
    private readonly string _pluginsDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranscodingSettingsDtoValidatorTests"/> class.
    /// </summary>
    public TranscodingSettingsDtoValidatorTests()
    {
        _sandboxRoot = Path.Combine(Path.GetTempPath(), "lumina-transcoding-validator-tests-" + Guid.NewGuid().ToString("N"));
        _mediaRoot = Path.Combine(_sandboxRoot, "media");
        _validCacheDirectory = Path.Combine(_sandboxRoot, "cache");
        _pluginsDirectory = Path.Combine(_sandboxRoot, "plugins");
        Directory.CreateDirectory(_mediaRoot);
        Directory.CreateDirectory(_validCacheDirectory);
        Directory.CreateDirectory(_pluginsDirectory);

        // The real path service is used, with the real path strategy of the current platform, so containment follows the file system rules of the host.
        IPlatformContext platformContext = Substitute.For<IPlatformContext>();
        IPathStrategy pathStrategy = OperatingSystem.IsWindows()
            ? new WindowsPathStrategy(new FileSystem())
            : new UnixPathStrategy(new FileSystem());
        platformContext.PathStrategy.Returns(pathStrategy);
        IPlatformContextManager platformContextManager = Substitute.For<IPlatformContextManager>();
        platformContextManager.GetCurrentContext().Returns(platformContext);
        IPathService pathService = new PathService(platformContextManager);

        // An absolute root directory pins the media root that is served by the media route, because Path.Combine returns its second argument when it is rooted.
        MediaSettingsDto mediaSettings = _mediaSettingsDtoFixture.Create(rootDirectory: _mediaRoot);
        PluginsSettingsDto pluginsSettings = _pluginsSettingsDtoFixture.Create(directory: _pluginsDirectory);
        _validator = new TranscodingSettingsDtoValidator(Options.Create(mediaSettings), Options.Create(pluginsSettings), pathService);
    }

    [Fact]
    public void Validate_WhenSettingsAreValid_ShouldNotHaveAnyValidationError()
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings();

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(null)] // a missing ffmpeg path
    [InlineData("")] // an empty ffmpeg path
    [InlineData("   ")] // a whitespace ffmpeg path
    public void Validate_WhenFfmpegPathIsEmpty_ShouldHaveFfmpegPathCannotBeEmptyError(string? ffmpegPath)
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(ffmpegPath: ffmpegPath, includeFfmpegPath: ffmpegPath is not null);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Contains(result, error => error.Description == Errors.Configuration.TranscodingFfmpegPathCannotBeEmpty.Description);
    }

    [Theory]
    [InlineData(0)] // a job count of zero disables normalization
    [InlineData(-1)] // a negative job count disables normalization
    public void Validate_WhenMaxConcurrentJobsIsNotPositive_ShouldHaveMaxConcurrentJobsMustBePositiveError(int maxConcurrentJobs)
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(maxConcurrentJobs: maxConcurrentJobs, maxConcurrentJobsPerUser: 1);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Contains(result, error => error.Description == Errors.Configuration.TranscodingMaxConcurrentJobsMustBePositive.Description);
    }

    [Fact]
    public void Validate_WhenMaxConcurrentJobsIsTooLarge_ShouldHaveMaxConcurrentJobsIsTooLargeError()
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(maxConcurrentJobs: 65, maxConcurrentJobsPerUser: 1);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Contains(result, error => error.Description == Errors.Configuration.TranscodingMaxConcurrentJobsIsTooLarge.Description);
    }

    [Fact]
    public void Validate_WhenMaxConcurrentJobsIsTheMaximum_ShouldNotHaveMaxConcurrentJobsErrors()
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(maxConcurrentJobs: 64, maxConcurrentJobsPerUser: 64);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.DoesNotContain(result, error => error.Description == Errors.Configuration.TranscodingMaxConcurrentJobsIsTooLarge.Description);
        Assert.DoesNotContain(result, error => error.Description == Errors.Configuration.TranscodingMaxConcurrentJobsMustBePositive.Description);
    }

    [Theory]
    [InlineData(0)] // a per-user job count of zero never admits a normalization
    [InlineData(-1)] // a negative per-user job count never admits a normalization
    public void Validate_WhenMaxConcurrentJobsPerUserIsNotPositive_ShouldHaveMaxConcurrentJobsPerUserMustBePositiveError(int maxConcurrentJobsPerUser)
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(maxConcurrentJobs: 8, maxConcurrentJobsPerUser: maxConcurrentJobsPerUser);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Contains(result, error => error.Description == Errors.Configuration.TranscodingMaxConcurrentJobsPerUserMustBePositive.Description);
    }

    [Fact]
    public void Validate_WhenMaxConcurrentJobsPerUserIsTooLarge_ShouldHaveMaxConcurrentJobsPerUserIsTooLargeError()
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(maxConcurrentJobs: 64, maxConcurrentJobsPerUser: 65);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Contains(result, error => error.Description == Errors.Configuration.TranscodingMaxConcurrentJobsPerUserIsTooLarge.Description);
    }

    [Fact]
    public void Validate_WhenMaxConcurrentJobsPerUserExceedsTheGlobalLimit_ShouldHaveMaxConcurrentJobsPerUserExceedsGlobalError()
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(maxConcurrentJobs: 8, maxConcurrentJobsPerUser: 16);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Contains(result, error => error.Description == Errors.Configuration.TranscodingMaxConcurrentJobsPerUserExceedsGlobal.Description);
        Assert.DoesNotContain(result, error => error.Description == Errors.Configuration.TranscodingMaxConcurrentJobsPerUserIsTooLarge.Description);
    }

    [Theory]
    [InlineData(0)] // a cache with no capacity cannot hold a normalized track
    [InlineData(-1)] // a negative cache size is not a capacity
    public void Validate_WhenMaxCacheBytesIsNotPositive_ShouldHaveMaxCacheBytesMustBePositiveError(long maxCacheBytes)
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(maxCacheBytes: maxCacheBytes);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Contains(result, error => error.Description == Errors.Configuration.TranscodingMaxCacheBytesMustBePositive.Description);
    }

    [Theory]
    [InlineData(0)] // a job timeout of zero stops every job immediately
    [InlineData(-1)] // a negative job timeout stops every job immediately
    public void Validate_WhenJobTimeoutSecondsIsNotPositive_ShouldHaveJobTimeoutMustBePositiveError(int jobTimeoutSeconds)
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(jobTimeoutSeconds: jobTimeoutSeconds);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Contains(result, error => error.Description == Errors.Configuration.TranscodingJobTimeoutMustBePositive.Description);
    }

    [Fact]
    public void Validate_WhenJobTimeoutSecondsIsTooLong_ShouldHaveJobTimeoutIsTooLongError()
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(jobTimeoutSeconds: 86_401);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Contains(result, error => error.Description == Errors.Configuration.TranscodingJobTimeoutIsTooLong.Description);
    }

    [Fact]
    public void Validate_WhenJobTimeoutSecondsIsTheMaximum_ShouldNotHaveJobTimeoutErrors()
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(jobTimeoutSeconds: 86_400);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.DoesNotContain(result, error => error.Description == Errors.Configuration.TranscodingJobTimeoutIsTooLong.Description);
        Assert.DoesNotContain(result, error => error.Description == Errors.Configuration.TranscodingJobTimeoutMustBePositive.Description);
    }

    [Theory]
    [InlineData(0)] // an eviction grace of zero protects nothing
    [InlineData(-1)] // a negative eviction grace protects nothing
    public void Validate_WhenEvictionGraceSecondsIsNotPositive_ShouldHaveEvictionGraceSecondsMustBePositiveError(int evictionGraceSeconds)
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(evictionGraceSeconds: evictionGraceSeconds);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Contains(result, error => error.Description == Errors.Configuration.TranscodingEvictionGraceSecondsMustBePositive.Description);
    }

    [Fact]
    public void Validate_WhenEvictionGraceSecondsIsTooLong_ShouldHaveEvictionGraceSecondsIsTooLongError()
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(evictionGraceSeconds: 86_401);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Contains(result, error => error.Description == Errors.Configuration.TranscodingEvictionGraceSecondsIsTooLong.Description);
    }

    [Fact]
    public void Validate_WhenEvictionGraceSecondsIsTheMaximum_ShouldNotHaveEvictionGraceSecondsErrors()
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(evictionGraceSeconds: 86_400);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.DoesNotContain(result, error => error.Description == Errors.Configuration.TranscodingEvictionGraceSecondsIsTooLong.Description);
        Assert.DoesNotContain(result, error => error.Description == Errors.Configuration.TranscodingEvictionGraceSecondsMustBePositive.Description);
    }

    [Fact]
    public void Validate_WhenCacheDirectoryIsMalformed_ShouldHaveCacheDirectoryIsInvalidError()
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(cacheDirectory: "bad\0path");

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Contains(result, error => error.Description == Errors.Configuration.TranscodingCacheDirectoryIsInvalid.Description);
        Assert.DoesNotContain(result, error => error.Description == Errors.Configuration.TranscodingCacheDirectoryIsInsideMediaRoot.Description);
        Assert.DoesNotContain(result, error => error.Description == Errors.Configuration.TranscodingCacheDirectoryContainsMediaRoot.Description);
        Assert.DoesNotContain(result, error => error.Description == Errors.Configuration.TranscodingCacheDirectoryIsUnsafeLocation.Description);
    }

    [Fact]
    public void Validate_WhenCacheDirectoryIsInsideTheMediaRoot_ShouldHaveCacheDirectoryIsInsideMediaRootError()
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(cacheDirectory: Path.Combine(_mediaRoot, "cache"));

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Contains(result, error => error.Description == Errors.Configuration.TranscodingCacheDirectoryIsInsideMediaRoot.Description);
        Assert.DoesNotContain(result, error => error.Description == Errors.Configuration.TranscodingCacheDirectoryContainsMediaRoot.Description);
    }

    [Fact]
    public void Validate_WhenCacheDirectoryContainsTheMediaRoot_ShouldHaveCacheDirectoryContainsMediaRootError()
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(cacheDirectory: _sandboxRoot);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Contains(result, error => error.Description == Errors.Configuration.TranscodingCacheDirectoryContainsMediaRoot.Description);
        Assert.DoesNotContain(result, error => error.Description == Errors.Configuration.TranscodingCacheDirectoryIsInsideMediaRoot.Description);
    }

    [Fact]
    public void Validate_WhenCacheDirectoryIsAFileSystemRoot_ShouldHaveCacheDirectoryIsUnsafeLocationError()
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(cacheDirectory: Path.GetPathRoot(_mediaRoot)!);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Contains(result, error => error.Description == Errors.Configuration.TranscodingCacheDirectoryIsUnsafeLocation.Description);
    }

    [Fact]
    public void Validate_WhenCacheDirectoryIsTheApplicationBaseDirectory_ShouldHaveCacheDirectoryIsUnsafeLocationError()
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(cacheDirectory: AppContext.BaseDirectory);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Contains(result, error => error.Description == Errors.Configuration.TranscodingCacheDirectoryIsUnsafeLocation.Description);
    }

    [Fact]
    public void Validate_WhenCacheDirectoryIsTheLogsDirectory_ShouldHaveCacheDirectoryIsUnsafeLocationError()
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(cacheDirectory: Path.Combine(AppContext.BaseDirectory, "logs"));

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Contains(result, error => error.Description == Errors.Configuration.TranscodingCacheDirectoryIsUnsafeLocation.Description);
    }

    [Fact]
    public void Validate_WhenCacheDirectoryIsThePluginsDirectory_ShouldHaveCacheDirectoryIsUnsafeLocationError()
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(cacheDirectory: _pluginsDirectory);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Contains(result, error => error.Description == Errors.Configuration.TranscodingCacheDirectoryIsUnsafeLocation.Description);
    }

    [Fact]
    public void Validate_WhenCacheDirectoryIsAValidExternalDirectory_ShouldNotHaveCacheDirectoryErrors()
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(cacheDirectory: _validCacheDirectory);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Empty(result);
    }

    [Theory]
    [InlineData(null)] // no configured directory, so the default temporary directory is used
    [InlineData("   ")] // a whitespace directory, so the default temporary directory is used
    public void Validate_WhenCacheDirectoryIsNotConfigured_ShouldOnlyValidateTheDefaultDirectory(string? cacheDirectory)
    {
        // Arrange
        TranscodingSettingsDto model = CreateSettings(cacheDirectory: cacheDirectory, includeCacheDirectory: cacheDirectory is not null);

        // Act
        List<Error> result = _validator.Validate(model);

        // Assert
        Assert.Empty(result);
    }

    /// <summary>
    /// Releases the temporary directories the containment tests use.
    /// </summary>
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_sandboxRoot))
                Directory.Delete(_sandboxRoot, recursive: true);
        }
        catch (IOException)
        {
            // A temporary directory that cannot be removed does not fail the test.
        }
    }

    /// <summary>
    /// Creates settings that are valid unless an argument overrides a single rule under test.
    /// </summary>
    /// <param name="ffmpegPath">The file name or path of the ffmpeg binary.</param>
    /// <param name="maxConcurrentJobs">The maximum number of normalization jobs that may run at the same time.</param>
    /// <param name="maxConcurrentJobsPerUser">The maximum number of normalization jobs a single user may have running at the same time.</param>
    /// <param name="maxCacheBytes">The maximum total size of the normalized audio cache, in bytes.</param>
    /// <param name="jobTimeoutSeconds">The maximum duration of a single normalization job, in seconds.</param>
    /// <param name="evictionGraceSeconds">The period during which a cache entry is protected from eviction, in seconds.</param>
    /// <param name="cacheDirectory">The directory where normalized audio is cached.</param>
    /// <param name="includeCacheDirectory">Whether the cache directory should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeFfmpegPath">Whether the ffmpeg path should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created settings.</returns>
    private TranscodingSettingsDto CreateSettings(
        string? ffmpegPath = "ffmpeg",
        int? maxConcurrentJobs = 8,
        int? maxConcurrentJobsPerUser = 4,
        long? maxCacheBytes = 2L * 1024 * 1024 * 1024,
        int? jobTimeoutSeconds = 900,
        int? evictionGraceSeconds = 120,
        string? cacheDirectory = null,
        bool includeCacheDirectory = true,
        bool includeFfmpegPath = true)
    {
        return _transcodingSettingsDtoFixture.Create(
            ffmpegPath: ffmpegPath,
            maxConcurrentJobs: maxConcurrentJobs,
            maxConcurrentJobsPerUser: maxConcurrentJobsPerUser,
            maxCacheBytes: maxCacheBytes,
            jobTimeoutSeconds: jobTimeoutSeconds,
            evictionGraceSeconds: evictionGraceSeconds,
            cacheDirectory: cacheDirectory ?? _validCacheDirectory,
            includeCacheDirectory: includeCacheDirectory,
            includeFfmpegPath: includeFfmpegPath);
    }
}
