#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.Libraries;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.Libraries;

/// <summary>
/// Fixture class for generating <see cref="LibraryDto"/> test data.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryDtoFixture
{
    /// <summary>
    /// Creates a new <see cref="LibraryDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional identifier of the media library.</param>
    /// <param name="title">Optional title of the library.</param>
    /// <param name="libraryType">Optional type of the library.</param>
    /// <param name="isEnabled">Whether the media library is enabled or not.</param>
    /// <param name="contentLocations">Optional collection of directories that contain the library files.</param>
    /// <returns>A configured <see cref="LibraryDto"/> instance.</returns>
    public LibraryDto Create(Guid? id = null, string? title = null, string? libraryType = null, bool? isEnabled = null, List<string>? contentLocations = null)
    {
        Faker faker = new();
        return new LibraryDto
        {
            Id = id ?? Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Title = title ?? faker.Commerce.Department(),
            LibraryType = libraryType ?? faker.PickRandom("Book", "Video", "Photo"),
            CoverImage = faker.Image.PicsumUrl(),
            ContentLocations = contentLocations ?? [faker.System.DirectoryPath()],
            IsEnabled = isEnabled ?? faker.Random.Bool()
        };
    }

    /// <summary>
    /// Creates multiple <see cref="LibraryDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryDto"/> instances.</returns>
    public List<LibraryDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
