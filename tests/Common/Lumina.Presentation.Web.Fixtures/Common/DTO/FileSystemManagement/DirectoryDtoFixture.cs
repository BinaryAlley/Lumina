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
/// Fixture class for the <see cref="DirectoryDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class DirectoryDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="DirectoryDto"/>.
    /// </summary>
    /// <param name="path">Optional. The full path to the directory.</param>
    /// <param name="name">Optional. The name of the directory.</param>
    /// <param name="items">Optional. The children items of the directory.</param>
    /// <param name="includeItems">Whether the directory should include children items, or an empty collection.</param>
    /// <returns>The created <see cref="DirectoryDto"/>.</returns>
    public DirectoryDto Create(
        string? path = null,
        string? name = null,
        List<FileSystemItemDto>? items = null,
        bool includeItems = false)
    {
        string generatedPath = _faker.System.DirectoryPath();
        return new DirectoryDto
        {
            Path = path ?? generatedPath,
            Name = name ?? Path.GetFileName(generatedPath),
            DateCreated = _faker.Date.Past(),
            DateModified = _faker.Date.Recent(),
            Items = includeItems ? (items ?? []) : []
        };
    }

    /// <summary>
    /// Creates a list of <see cref="DirectoryDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="DirectoryDto"/> instances.</returns>
    public List<DirectoryDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
