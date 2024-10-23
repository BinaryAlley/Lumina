#region ========================================================================= USING =====================================================================================
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using FluentAssertions;
using Lumina.Application.Common.Mapping.FileSystemManagement.Files;
using Lumina.Application.Core.FileSystemManagement.Files.Queries.GetTreeFiles;
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
    private readonly IFixture _fixture;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetTreeFilesRequestMappingTests"/> class.
    /// </summary>
    public GetTreeFilesRequestMappingTests()
    {
        _fixture = new Fixture().Customize(new AutoNSubstituteCustomization());
    }

    [Fact]
    public void ToQuery_WhenMappingGetTreeFilesRequest_ShouldMapCorrectly()
    {
        // Arrange
        GetTreeFilesRequest request = _fixture.Create<GetTreeFilesRequest>();

        // Act
        GetTreeFilesQuery result = request.ToQuery();

        // Assert
        result.Should().NotBeNull();
        result.Path.Should().Be(request.Path);
        result.IncludeHiddenElements.Should().Be(request.IncludeHiddenElements);
    }
}
