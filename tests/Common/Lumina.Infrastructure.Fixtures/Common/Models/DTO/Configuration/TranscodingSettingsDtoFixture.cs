#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Infrastructure.Common.Models.DTO.Configuration;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Infrastructure.Fixtures.Common.Models.DTO.Configuration;

/// <summary>
/// Fixture class for the <see cref="TranscodingSettingsDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TranscodingSettingsDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="TranscodingSettingsDto"/>.
    /// </summary>
    /// <param name="ffmpegPath">Optional file name or path of the ffmpeg binary.</param>
    /// <param name="maxConcurrentJobs">Optional maximum number of normalization jobs that may run at the same time.</param>
    /// <param name="maxConcurrentJobsPerUser">Optional maximum number of normalization jobs a single user may have running at the same time.</param>
    /// <param name="maxCacheBytes">Optional maximum total size of the normalized audio cache, in bytes.</param>
    /// <param name="jobTimeoutSeconds">Optional maximum duration of a single normalization job, in seconds.</param>
    /// <param name="evictionGraceSeconds">Optional period during which a cache entry is protected from eviction, in seconds.</param>
    /// <param name="cacheDirectory">
    /// Optional directory where normalized audio is cached. When it is not supplied, a generated directory is used. When
    /// <paramref name="includeCacheDirectory"/> is <see langword="false"/>, the property stays <see langword="null"/>, which makes the pipeline use the
    /// default temporary directory.
    /// </param>
    /// <param name="includeCacheDirectory">Whether to generate the optional cache directory, defaulting to <see langword="true"/>.</param>
    /// <returns>A configured <see cref="TranscodingSettingsDto"/> instance.</returns>
    public TranscodingSettingsDto Create(
        string? ffmpegPath = null,
        int? maxConcurrentJobs = null,
        int? maxConcurrentJobsPerUser = null,
        long? maxCacheBytes = null,
        int? jobTimeoutSeconds = null,
        int? evictionGraceSeconds = null,
        string? cacheDirectory = null,
        bool includeCacheDirectory = true)
    {
        return new TranscodingSettingsDto
        {
            FfmpegPath = ffmpegPath ?? _faker.System.FileName("exe"),
            MaxConcurrentJobs = maxConcurrentJobs ?? _faker.Random.Int(1, 16),
            MaxConcurrentJobsPerUser = maxConcurrentJobsPerUser ?? _faker.Random.Int(1, 8),
            MaxCacheBytes = maxCacheBytes ?? _faker.Random.Long(1_000_000_000, 8_589_934_592),
            JobTimeoutSeconds = jobTimeoutSeconds ?? _faker.Random.Int(60, 3600),
            EvictionGraceSeconds = evictionGraceSeconds ?? _faker.Random.Int(0, 600),
            CacheDirectory = includeCacheDirectory ? cacheDirectory ?? _faker.System.DirectoryPath() : null
        };
    }

    /// <summary>
    /// Creates a list of <see cref="TranscodingSettingsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="TranscodingSettingsDto"/> instances.</returns>
    public List<TranscodingSettingsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
