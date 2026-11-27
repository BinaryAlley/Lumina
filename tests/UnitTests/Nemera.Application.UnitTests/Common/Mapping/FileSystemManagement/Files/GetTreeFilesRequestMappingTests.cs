#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.FileSystemManagement.Files;
using Nemera.Application.Core.FileSystemManagement.Files.Queries.GetTreeFiles;
using Nemera.Contracts.Fixtures.Core.Requests.FileSystemManagement.Files;
using Nemera.Contracts.Requests.FileSystemManagement.Files;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.FileSystemManagement.Files;

/// <summary>
/// Contains unit tests for the <see cref="GetTreeFilesRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTreeFilesRequestMappingTests
{
    private readonly GetTreeFilesRequestFixture _getTreeFilesRequestFixture = new();

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
        Assert.Equal(request.ShouldIncludeHiddenElements, result.ShouldIncludeHiddenElements);
    }
}
