#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.FileSystemManagement.Path;

/// <summary>
/// Fixture class for the <see cref="PathExistsResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PathExistsResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PathExistsResponse"/>.
    /// </summary>
    /// <param name="exists">Optional. Whether the path exists.</param>
    /// <returns>The created <see cref="PathExistsResponse"/>.</returns>
    public PathExistsResponse Create(
        bool? exists = null)
    {
        return new PathExistsResponse(exists ?? _faker.Random.Bool());
    }

    /// <summary>
    /// Creates a list of <see cref="PathExistsResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<PathExistsResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
