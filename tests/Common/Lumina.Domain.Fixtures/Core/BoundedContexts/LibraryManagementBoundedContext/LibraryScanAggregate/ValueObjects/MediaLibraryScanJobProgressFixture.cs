#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="MediaLibraryScanJobProgress"/> value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaLibraryScanJobProgressFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="MediaLibraryScanJobProgress"/>.
    /// </summary>
    /// <param name="completedItems">Optional. The number of completed items of the scan job.</param>
    /// <param name="totalItems">Optional. The total number of items of the scan job.</param>
    /// <param name="currentOperation">Optional. The current operation being performed by the scan job.</param>
    /// <returns>The created <see cref="MediaLibraryScanJobProgress"/>.</returns>
    public MediaLibraryScanJobProgress Create(
        int? completedItems = null,
        int? totalItems = null,
        string? currentOperation = null)
    {
        Result<MediaLibraryScanJobProgress> progress = MediaLibraryScanJobProgress.Create(
            completedItems ?? _faker.Random.Number(0, 100),
            totalItems ?? _faker.Random.Number(100, 200),
            currentOperation ?? _faker.Random.Words());

        return progress.Value;
    }

    /// <summary>
    /// Creates a list of <see cref="MediaLibraryScanJobProgress"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaLibraryScanJobProgress"/> instances.</returns>
    public List<MediaLibraryScanJobProgress> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
