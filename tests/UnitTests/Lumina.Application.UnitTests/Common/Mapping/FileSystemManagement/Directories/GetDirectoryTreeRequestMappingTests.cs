#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Lumina.Application.Common.Mapping.FileSystemManagement.Directories;
using Lumina.Application.Core.FileSystemManagement.Directories.Queries.GetDirectoryTree;
using Lumina.Contracts.Requests.FileSystemManagement.Directories;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.FileSystemManagement.Directories;

/// <summary>
/// Contains unit tests for the <see cref="GetDirectoryTreeRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class GetDirectoryTreeRequestMappingTests
{
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetDirectoryTreeRequestMappingTests"/> class.
    /// </summary>
    public GetDirectoryTreeRequestMappingTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void ToQuery_WhenMappingGetDirectoryTreeRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetDirectoryTreeRequest request = _fixture.Create<GetDirectoryTreeRequest>();

        // Act
        GetDirectoryTreeQuery result = request.ToQuery();

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(request.Path);
        result.IncludeFiles.Should().Be(request.IncludeFiles);
        result.IncludeHiddenElements.Should().Be(request.IncludeHiddenElements);
    }
}
