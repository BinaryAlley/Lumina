#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.FileSystemManagement.Files;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Files;

/// <summary>
/// Fixture class for the <see cref="GetFilesRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFilesRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a <see cref="GetFilesRequest"/> with default or random values.
    /// </summary>
    /// <param name="path">Optional. The file system path for which to get the files.</param>
    /// <param name="includeHiddenElements">Optional. Whether to include hidden file system elements.</param>
    /// <returns>The created <see cref="GetFilesRequest"/>.</returns>
    public GetFilesRequest Create(string? path = null, bool? includeHiddenElements = null)
    {
        return new GetFilesRequest(
            Path: path ?? _faker.System.FilePath(),
            IncludeHiddenElements: includeHiddenElements ?? _faker.Random.Bool()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="GetFilesRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetFilesRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
