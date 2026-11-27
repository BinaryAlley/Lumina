#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Requests.Library.Statistics;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Library.Statistics;

/// <summary>
/// Fixture class for the <see cref="ClearResumePointsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ClearResumePointsRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="ClearResumePointsRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="userId">Optional Id of the user whose resume points are deleted.</param>
    /// <param name="libraryId">Optional Id of the media library to restrict the deletion to.</param>
    /// <param name="includeUserId">Whether the user Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLibraryId">Whether the library Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="ClearResumePointsRequest"/> instance.</returns>
    public ClearResumePointsRequest Create(
        string? userId = null,
        string? libraryId = null,
        bool includeUserId = true,
        bool includeLibraryId = true)
    {
        return new ClearResumePointsRequest(
            UserId: includeUserId ? (userId ?? _faker.Random.Guid().ToString()) : null,
            LibraryId: includeLibraryId ? (libraryId ?? _faker.Random.Guid().ToString()) : null
        );
    }

    /// <summary>
    /// Creates multiple <see cref="ClearResumePointsRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="ClearResumePointsRequest"/> instances.</returns>
    public List<ClearResumePointsRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
