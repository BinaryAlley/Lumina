#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.Management;

/// <summary>
/// Fixture class for the <see cref="MediaLibraryScanProgressResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanProgressResponseFixture
{
    private readonly Faker _faker = new();
    private readonly MediaLibraryScanJobProgressResponseFixture _mediaLibraryScanJobProgressResponseFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="MediaLibraryScanProgressResponse"/>.
    /// </summary>
    /// <param name="scanId">Optional. The Id of the media library scan.</param>
    /// <param name="userId">Optional. The Id of the user initiating the media library scan.</param>
    /// <param name="libraryId">Optional. The Id of the media library that is scanned.</param>
    /// <param name="totalJobs">Optional. The total number of jobs to be processed by the scan.</param>
    /// <param name="completedJobs">Optional. The number of jobs that have been processed.</param>
    /// <param name="currentJobProgress">Optional. The progress of the currently processing job.</param>
    /// <param name="status">Optional. The status of the scan.</param>
    /// <param name="overallProgressPercentage">Optional. The overall progress percentage of the scan.</param>
    /// <returns>The created <see cref="MediaLibraryScanProgressResponse"/>.</returns>
    public MediaLibraryScanProgressResponse Create(
        Guid? scanId = null,
        Guid? userId = null,
        Guid? libraryId = null,
        int? totalJobs = null,
        int? completedJobs = null,
        MediaLibraryScanJobProgressResponse? currentJobProgress = null,
        string? status = null,
        decimal? overallProgressPercentage = null)
    {
        return new MediaLibraryScanProgressResponse(
            scanId ?? Guid.NewGuid(),
            userId ?? Guid.NewGuid(),
            libraryId ?? Guid.NewGuid(),
            totalJobs ?? _faker.Random.Int(1, 10),
            completedJobs ?? _faker.Random.Int(0, 10),
            currentJobProgress ?? _mediaLibraryScanJobProgressResponseFixture.Create(),
            status ?? _faker.Lorem.Word(),
            overallProgressPercentage ?? _faker.Random.Decimal(0m, 100m)
        );
    }

    /// <summary>
    /// Creates a list of <see cref="MediaLibraryScanProgressResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<MediaLibraryScanProgressResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
