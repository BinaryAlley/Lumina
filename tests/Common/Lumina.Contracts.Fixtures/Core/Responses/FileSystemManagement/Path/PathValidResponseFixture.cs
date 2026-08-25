#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.FileSystemManagement.Path;

/// <summary>
/// Fixture class for the <see cref="PathValidResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PathValidResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PathValidResponse"/>.
    /// </summary>
    /// <param name="isValid">Optional. Whether the path is valid.</param>
    /// <returns>The created <see cref="PathValidResponse"/>.</returns>
    public PathValidResponse Create(
        bool? isValid = null)
    {
        return new PathValidResponse(isValid ?? _faker.Random.Bool());
    }

    /// <summary>
    /// Creates a list of <see cref="PathValidResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<PathValidResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
