#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;

/// <summary>
/// Fixture class for generating <see cref="LibraryScanProgressDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryScanProgressDtoFixture
{
    private readonly Faker _faker = new();

    private readonly LibraryScanJobProgressDtoFixture _libraryScanJobProgressDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="LibraryScanProgressDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="scanId">Optional unique identifier of the media library scan.</param>
    /// <param name="userId">Optional unique identifier of the user initiating the media library scan.</param>
    /// <param name="libraryId">Optional unique identifier of the media library that is scanned.</param>
    /// <param name="completedJobs">Optional number of completed jobs of the media library scan job.</param>
    /// <param name="totalJobs">Optional total number of jobs of the media library scan job.</param>
    /// <param name="status">Optional status of the media library scan.</param>
    /// <param name="includeCurrentJobProgress">Whether the current job progress should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="LibraryScanProgressDto"/> instance.</returns>
    public LibraryScanProgressDto Create(
        Guid? scanId = null,
        Guid? userId = null,
        Guid? libraryId = null,
        int? completedJobs = null,
        int? totalJobs = null,
        string? status = null,
        bool includeCurrentJobProgress = false)
    {
        int completed = completedJobs ?? _faker.Random.Int(0, 10);
        int total = totalJobs ?? _faker.Random.Int(completed, 10);
        return new LibraryScanProgressDto
        {
            ScanId = scanId ?? Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            LibraryId = libraryId ?? Guid.NewGuid(),
            CompletedJobs = completed,
            TotalJobs = total,
            CurrentJobProgress = includeCurrentJobProgress ? _libraryScanJobProgressDtoFixture.Create() : null,
            Status = status ?? _faker.PickRandom("Running", "Completed", "Failed"),
            OverallProgressPercentage = total > 0 ? (decimal)completed / total * 100 : 0
        };
    }

    /// <summary>
    /// Creates multiple <see cref="LibraryScanProgressDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryScanProgressDto"/> instances.</returns>
    public List<LibraryScanProgressDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
