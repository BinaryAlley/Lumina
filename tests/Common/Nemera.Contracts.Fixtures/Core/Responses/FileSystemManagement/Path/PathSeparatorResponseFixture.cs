#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.FileSystemManagement.Path;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.FileSystemManagement.Path;

/// <summary>
/// Fixture class for the <see cref="PathSeparatorResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class PathSeparatorResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PathSeparatorResponse"/>.
    /// </summary>
    /// <param name="separator">Optional. The path separator.</param>
    /// <returns>The created <see cref="PathSeparatorResponse"/>.</returns>
    public PathSeparatorResponse Create(string? separator = null)
    {
        return new PathSeparatorResponse(separator ?? _faker.System.DirectoryPath());
    }

    /// <summary>
    /// Creates a list of <see cref="PathSeparatorResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<PathSeparatorResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
