#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.FileSystemManagement.Directories;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Directories;

/// <summary>
/// Fixture class for the <see cref="GetDirectoriesRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoriesRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a <see cref="GetDirectoriesRequest"/> with default or random values.
    /// </summary>
    /// <param name="path">Optional. The file system path for which to get the directories.</param>
    /// <param name="includeHiddenElements">Optional. Whether to include hidden file system elements.</param>
    /// <returns>The created <see cref="GetDirectoriesRequest"/>.</returns>
    public GetDirectoriesRequest Create(string? path = null, bool? includeHiddenElements = null)
    {
        return new GetDirectoriesRequest(
            Path: path ?? _faker.System.FilePath(),
            IncludeHiddenElements: includeHiddenElements ?? _faker.Random.Bool()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="GetDirectoriesRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetDirectoriesRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
