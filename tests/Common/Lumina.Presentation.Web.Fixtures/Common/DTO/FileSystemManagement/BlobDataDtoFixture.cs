#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.FileSystemManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.FileSystemManagement;

/// <summary>
/// Fixture class for the <see cref="BlobDataDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class BlobDataDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="BlobDataDto"/>.
    /// </summary>
    /// <param name="data">Optional. The binary data as a byte array.</param>
    /// <param name="contentType">Optional. The content type (MIME type) of the blob data.</param>
    /// <returns>The created <see cref="BlobDataDto"/>.</returns>
    public BlobDataDto Create(
        byte[]? data = null, 
        string? contentType = null)
    {
        return new BlobDataDto
        {
            Data = data ?? _faker.Random.Bytes(32),
            ContentType = contentType ?? _faker.System.MimeType()
        };
    }

    /// <summary>
    /// Creates a list of <see cref="BlobDataDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="BlobDataDto"/> instances.</returns>
    public List<BlobDataDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
