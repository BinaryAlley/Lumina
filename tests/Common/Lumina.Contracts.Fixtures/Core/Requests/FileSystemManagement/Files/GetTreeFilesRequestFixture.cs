#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.FileSystemManagement.Files;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Files;

/// <summary>
/// Fixture class for the <see cref="GetTreeFilesRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTreeFilesRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a <see cref="GetTreeFilesRequest"/> with default or random values.
    /// </summary>
    /// <param name="path">Optional. The file system path for which to get the tree files.</param>
    /// <param name="includeHiddenElements">Optional. Whether to include hidden file system elements.</param>
    /// <returns>The created <see cref="GetTreeFilesRequest"/>.</returns>
    public GetTreeFilesRequest Create(string? path = null, bool? includeHiddenElements = null)
    {
        return new GetTreeFilesRequest(
            Path: path ?? _faker.System.FilePath(),
            IncludeHiddenElements: includeHiddenElements ?? _faker.Random.Bool()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="GetTreeFilesRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetTreeFilesRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
