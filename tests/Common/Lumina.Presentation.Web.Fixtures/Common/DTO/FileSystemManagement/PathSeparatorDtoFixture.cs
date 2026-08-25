#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;

/// <summary>
/// Fixture class for generating <see cref="PathSeparatorDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class PathSeparatorDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="PathSeparatorDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="separator">Optional file system path separator.</param>
    /// <returns>A configured <see cref="PathSeparatorDto"/> instance.</returns>
    public PathSeparatorDto Create(
        string? separator = null)
    {
        return new PathSeparatorDto(
            Separator: separator ?? (_faker.Random.Bool() ? "\\" : "/")
        );
    }

    /// <summary>
    /// Creates multiple <see cref="PathSeparatorDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PathSeparatorDto"/> instances.</returns>
    public List<PathSeparatorDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
