#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;

/// <summary>
/// Fixture class for generating <see cref="PathSegmentDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class PathSegmentDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="PathSegmentDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="path">Optional. The returned path.</param>
    /// <returns>A configured <see cref="PathSegmentDto"/> instance.</returns>
    public PathSegmentDto Create(
        string? path = null)
    {
        return new PathSegmentDto
        {
            Path = path ?? _faker.System.FilePath()
        };
    }

    /// <summary>
    /// Creates multiple <see cref="PathSegmentDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PathSegmentDto"/> instances.</returns>
    public List<PathSegmentDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
