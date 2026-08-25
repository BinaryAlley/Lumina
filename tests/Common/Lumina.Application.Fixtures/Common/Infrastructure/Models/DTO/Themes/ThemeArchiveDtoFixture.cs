#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.Infrastructure.Models.DTO.Themes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.Infrastructure.Models.DTO.Themes;

/// <summary>
/// Fixture class for the <see cref="ThemeArchiveDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class ThemeArchiveDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="ThemeArchiveDto"/>.
    /// </summary>
    /// <param name="bytes">Optional. The bytes of the ZIP archive.</param>
    /// <param name="fileName">Optional. The file name to expose when downloading the archive.</param>
    /// <param name="contentType">Optional. The MIME content type of the archive.</param>
    /// <returns>The created <see cref="ThemeArchiveDto"/>.</returns>
    public ThemeArchiveDto Create(
        byte[]? bytes = null, 
        string? fileName = null, 
        string? contentType = null)
    {
        return new ThemeArchiveDto(
            bytes ?? _faker.Random.Bytes(10),
            fileName ?? _faker.System.FileName(),
            contentType ?? _faker.System.MimeType());
    }

    /// <summary>
    /// Creates a list of <see cref="ThemeArchiveDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<ThemeArchiveDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
