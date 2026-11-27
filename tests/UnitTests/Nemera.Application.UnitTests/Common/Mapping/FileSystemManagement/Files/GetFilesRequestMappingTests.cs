#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.FileSystemManagement.Files;
using Nemera.Application.Core.FileSystemManagement.Files.Queries.GetFiles;
using Nemera.Contracts.Fixtures.Core.Requests.FileSystemManagement.Files;
using Nemera.Contracts.Requests.FileSystemManagement.Files;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.FileSystemManagement.Files;

/// <summary>
/// Contains unit tests for the <see cref="GetFilesRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFilesRequestMappingTests
{
    private readonly GetFilesRequestFixture _getFilesRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingGetFilesRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetFilesRequest request = _getFilesRequestFixture.Create();

        // Act
        GetFilesQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Path, result.Path);
        Assert.Equal(request.ShouldIncludeHiddenElements, result.ShouldIncludeHiddenElements);
    }
}
