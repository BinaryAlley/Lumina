#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Path;

/// <summary>
/// Fixture class for the <see cref="CheckPathExistsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckPathExistsRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a <see cref="CheckPathExistsRequest"/> with default or random values.
    /// </summary>
    /// <param name="path">Optional. The path to check.</param>
    /// <param name="includeHiddenElements">Optional. Whether to include hidden elements.</param>
    /// <returns>The created <see cref="CheckPathExistsRequest"/>.</returns>
    public CheckPathExistsRequest Create(string? path = null, bool? includeHiddenElements = null)
    {
        return new CheckPathExistsRequest(
            Path: path ?? _faker.System.FilePath(),
            IncludeHiddenElements: includeHiddenElements ?? _faker.Random.Bool()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="CheckPathExistsRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<CheckPathExistsRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
