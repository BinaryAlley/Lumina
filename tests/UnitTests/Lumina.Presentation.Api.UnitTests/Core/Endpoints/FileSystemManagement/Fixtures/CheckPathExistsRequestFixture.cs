#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Fixtures;

/// <summary>
/// Fixture class for the <see cref="CheckPathExistsRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class CheckPathExistsRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a <see cref="CheckPathExistsRequest"/> with default or random values.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <param name="includeHiddenElements">Whether to include hidden elements.</param>
    /// <returns>The created <see cref="CheckPathExistsRequest"/>.</returns>
    public CheckPathExistsRequest Create(string? path = null, bool? includeHiddenElements = null)
    {
        return new CheckPathExistsRequest(
            Path: path ?? _faker.System.FilePath(),
            IncludeHiddenElements: includeHiddenElements ?? _faker.Random.Bool()
        );
    }
}
