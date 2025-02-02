#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Application.Common.Mapping.FileSystemManagement.Directories;
using Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetDirectories;
using Lumina.Contracts.Requests.FileSystemManagement.Directories;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.Directories;

/// <summary>
/// Contains unit tests for the <see cref="GetDirectoriesRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoriesRequestMappingTests
{
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoriesRequestMappingTests"/> class.
    /// </summary>
    public GetDirectoriesRequestMappingTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void ToQuery_WhenMappingGetDirectoriesRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetDirectoriesRequest request = _fixture.Create<GetDirectoriesRequest>();

        // Act
        GetDirectoriesQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Path, result.Path);
        Assert.Equal(request.IncludeHiddenElements, result.IncludeHiddenElements);
    }
}
