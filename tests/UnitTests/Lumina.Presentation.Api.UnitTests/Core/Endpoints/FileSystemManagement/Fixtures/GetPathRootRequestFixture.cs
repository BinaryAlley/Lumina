#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Fixtures;

/// <summary>
/// Fixture class for the <see cref="GetPathRootRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathRootRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a <see cref="GetPathRootRequest"/> with a default or random value.
    /// </summary>
    /// <param name="path">The path to get the root of.</param>
    /// <returns>The created <see cref="GetPathRootRequest"/>.</returns>
    public GetPathRootRequest Create(string? path = null)
    {
        return new GetPathRootRequest(
            Path: path ?? _faker.System.FilePath()
        );
    }
}
