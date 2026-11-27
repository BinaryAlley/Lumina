#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.Management;

/// <summary>
/// Fixture class for the <see cref="MediaLibraryScanJobProgressResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanJobProgressResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="MediaLibraryScanJobProgressResponse"/>.
    /// </summary>
    /// <param name="completedItems">Optional. The number of processed scan job items.</param>
    /// <param name="totalItems">Optional. The total number of scan job items.</param>
    /// <param name="currentOperation">Optional. The current processing operation in the scan job.</param>
    /// <param name="progressPercentage">Optional. The progress percentage of the scan job.</param>
    /// <returns>The created <see cref="MediaLibraryScanJobProgressResponse"/>.</returns>
    public MediaLibraryScanJobProgressResponse Create(
        int? completedItems = null,
        int? totalItems = null,
        string? currentOperation = null,
        decimal? progressPercentage = null)
    {
        return new MediaLibraryScanJobProgressResponse(
            completedItems ?? _faker.Random.Int(0, 100),
            totalItems ?? _faker.Random.Int(100, 1000),
            currentOperation ?? _faker.Lorem.Word(),
            progressPercentage ?? _faker.Random.Decimal(0m, 100m)
        );
    }

    /// <summary>
    /// Creates a list of <see cref="MediaLibraryScanJobProgressResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<MediaLibraryScanJobProgressResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
