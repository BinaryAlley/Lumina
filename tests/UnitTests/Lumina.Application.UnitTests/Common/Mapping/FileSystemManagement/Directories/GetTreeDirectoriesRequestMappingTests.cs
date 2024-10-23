#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Lumina.Application.Common.Mapping.FileSystemManagement.Directories;
using Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetTreeDirectories;
using Lumina.Contracts.Requests.FileSystemManagement.Directories;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.Directories;

/// <summary>
/// Contains unit tests for the <see cref="GetTreeDirectoriesRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetTreeDirectoriesRequestMappingTests
{
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeDirectoriesRequestMappingTests"/> class.
    /// </summary>
    public GetTreeDirectoriesRequestMappingTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void ToQuery_WhenMappingGetTreeDirectoriesRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetTreeDirectoriesRequest request = _fixture.Create<GetTreeDirectoriesRequest>();

        // Act
        GetTreeDirectoriesQuery result = request.ToQuery();

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(request.Path);
        result.IncludeHiddenElements.Should().Be(request.IncludeHiddenElements);
    }
}
