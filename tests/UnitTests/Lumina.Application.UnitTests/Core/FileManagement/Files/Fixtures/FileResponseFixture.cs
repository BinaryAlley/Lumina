#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.FileManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Core.FileManagement.Files.Fixtures;

/// <summary>
/// Fixture class for the <see cref="FileResponse"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class FileResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="FileResponse"/>.
    /// </summary>
    /// <returns>The created <see cref="FileResponse"/>.</returns>
    public FileResponse Create()
    {
        return new FileResponse(
            _faker.System.FilePath(),
            _faker.System.FileName(),
            _faker.Date.Past(),
            _faker.Date.Recent(),
            _faker.Random.Long()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="FileResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<FileResponse> CreateMany(int count = 3)
    {
        return Enumerable.Range(0, count).Select(_ => Create()).ToList();
    }
}
