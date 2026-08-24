#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;

/// <summary>
/// Fixture class for generating <see cref="FileDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="FileDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="path">Optional. The full path to the file.</param>
    /// <param name="name">Optional. The name of the file.</param>
    /// <param name="size">Optional. The size of the file, in bytes.</param>
    /// <returns>A configured <see cref="FileDto"/> instance.</returns>
    public FileDto Create(
        string? path = null, 
        string? name = null, 
        long? size = null)
    {
        string generatedPath = _faker.System.FilePath();
        return new FileDto
        {
            Path = path ?? generatedPath,
            Name = name ?? Path.GetFileName(generatedPath),
            DateCreated = _faker.Date.Past(),
            DateModified = _faker.Date.Recent(),
            Size = size ?? _faker.Random.Long(0, 10_000_000)
        };
    }

    /// <summary>
    /// Creates multiple <see cref="FileDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="FileDto"/> instances.</returns>
    public List<FileDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
