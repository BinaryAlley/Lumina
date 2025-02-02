#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using Lumina.Application.Common.Mapping.FileSystemManagement.Files;
using Lumina.Application.Core.FileSystemManagement.Files.Queries.GetFiles;
using Lumina.Contracts.Requests.FileSystemManagement.Files;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.Files;

/// <summary>
/// Contains unit tests for the <see cref="GetFilesRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetFilesRequestMappingTests
{
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetFilesRequestMappingTests"/> class.
    /// </summary>
    public GetFilesRequestMappingTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void ToQuery_WhenMappingGetFilesRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetFilesRequest request = _fixture.Create<GetFilesRequest>();

        // Act
        GetFilesQuery result = request.ToQuery();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Path, result.Path);
        Assert.Equal(request.IncludeHiddenElements, result.IncludeHiddenElements);
    }
}
