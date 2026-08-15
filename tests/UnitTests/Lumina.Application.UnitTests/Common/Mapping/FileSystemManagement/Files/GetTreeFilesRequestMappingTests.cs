#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.FileSystemManagement.Files;
using Lumina.Application.Core.FileSystemManagement.Files.Queries.GetTreeFiles;
using Lumina.Contracts.Fixtures.Core.Requests.FileSystemManagement.Files;
using Lumina.Contracts.Requests.FileSystemManagement.Files;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.Files;

/// <summary>
/// Contains unit tests for the <see cref="GetTreeFilesRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTreeFilesRequestMappingTests
{
    private readonly GetTreeFilesRequestFixture _getTreeFilesRequestFixture = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeFilesRequestMappingTests"/> class.
    /// </summary>
    public GetTreeFilesRequestMappingTests()
    {
    }

    [Fact]
    public void ToQuery_WhenMappingGetTreeFilesRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetTreeFilesRequest request = _getTreeFilesRequestFixture.Create();

        // Act
        GetTreeFilesQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Path, result.Path);
        Assert.Equal(request.IncludeHiddenElements, result.IncludeHiddenElements);
    }
}
