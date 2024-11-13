#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.FileSystemManagement.Path;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Presentation.Api.UnitTests.Core.Endpoints.FileSystemManagement.Fixtures;

/// <summary>
/// Fixture class for the <see cref="GetPathParentRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetPathParentRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a <see cref="GetPathParentRequest"/> with a default or random value.
    /// </summary>
    /// <param name="path">The path to get the parent of.</param>
    /// <returns>The created <see cref="GetPathParentRequest"/>.</returns>
    public GetPathParentRequest Create(string? path = null)
    {
        return new GetPathParentRequest(
            Path: path ?? _faker.System.FilePath()
        );
    }
}
