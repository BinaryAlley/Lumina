#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.Infrastructure.Models.DTO.Configuration;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.Infrastructure.Models.DTO.Configuration;

/// <summary>
/// Fixture class for the <see cref="MediaSettingsDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaSettingsDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="MediaSettingsDto"/>.
    /// </summary>
    /// <param name="rootDirectory">Optional. The root directory where media files are stored.</param>
    /// <param name="librariesDirectory">Optional. The directory where media library files are stored.</param>
    /// <param name="booksDirectory">Optional. The directory where the media item files are stored.</param>
    /// <returns>The created <see cref="MediaSettingsDto"/>.</returns>
    public MediaSettingsDto Create(
        string? rootDirectory = null, 
        string? librariesDirectory = null, 
        string? booksDirectory = null)
    {
        return new MediaSettingsDto
        {
            RootDirectory = rootDirectory ?? _faker.System.DirectoryPath(),
            LibrariesDirectory = librariesDirectory ?? _faker.System.DirectoryPath(),
            BooksDirectory = booksDirectory ?? _faker.System.DirectoryPath()
        };
    }

    /// <summary>
    /// Creates a list of <see cref="MediaSettingsDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MediaSettingsDto"/> instances.</returns>
    public List<MediaSettingsDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
