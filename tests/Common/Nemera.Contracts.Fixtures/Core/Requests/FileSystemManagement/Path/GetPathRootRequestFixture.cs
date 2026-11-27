#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Path;

/// <summary>
/// Fixture class for the <see cref="GetPathRootRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathRootRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a <see cref="GetPathRootRequest"/> with a default or random value.
    /// </summary>
    /// <param name="path">Optional. The path to get the root of.</param>
    /// <returns>The created <see cref="GetPathRootRequest"/>.</returns>
    public GetPathRootRequest Create(string? path = null)
    {
        return new GetPathRootRequest(
            Path: path ?? _faker.System.FilePath()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="GetPathRootRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<GetPathRootRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
