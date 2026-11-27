#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.MediaConsumption;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaConsumption;

/// <summary>
/// Fixture class for the <see cref="ClearResumePointsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearResumePointsRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ClearResumePointsRequest"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library to restrict the deletion to. Defaults to a random Id.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="ClearResumePointsRequest"/>.</returns>
    public ClearResumePointsRequest Create(
        string? libraryId = null,
        bool includeLibraryId = true)
    {
        return new ClearResumePointsRequest(
            includeLibraryId ? libraryId ?? _faker.Random.Guid().ToString() : null);
    }

    /// <summary>
    /// Creates a list of <see cref="ClearResumePointsRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ClearResumePointsRequest"/> instances.</returns>
    public List<ClearResumePointsRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
