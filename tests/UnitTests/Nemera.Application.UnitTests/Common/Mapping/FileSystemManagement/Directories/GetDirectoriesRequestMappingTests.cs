#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.Mapping.FileSystemManagement.Directories;
using Nemera.Application.Core.FileSystemManagement.Directories.Queries.GetDirectories;
using Nemera.Contracts.Fixtures.Core.Requests.FileSystemManagement.Directories;
using Nemera.Contracts.Requests.FileSystemManagement.Directories;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Nemera.Application.UnitTests.Common.Mapping.FileSystemManagement.Directories;

/// <summary>
/// Contains unit tests for the <see cref="GetDirectoriesRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoriesRequestMappingTests
{
    private readonly GetDirectoriesRequestFixture _getDirectoriesRequestFixture = new();

    [Fact]
    public void ToQuery_WhenMappingGetDirectoriesRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetDirectoriesRequest request = _getDirectoriesRequestFixture.Create();

        // Act
        GetDirectoriesQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Path, result.Path);
        Assert.Equal(request.ShouldIncludeHiddenElements, result.ShouldIncludeHiddenElements);
    }
}
