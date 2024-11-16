#region ========================================================================= USING =====================================================================================
using FluentAssertions;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Commands.AddLibrary;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Domain.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="AddMediaLibraryRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddMediaLibraryRequestMappingTests
{
    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        AddLibraryRequest request = new(
            "My Library",
            LibraryType.Book,
            ["C:/Books", "D:/Media/Books"]
        );

        // Act
        AddLibraryCommand result = request.ToCommand(userId);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Title.Should().Be(request.Title);
        result.LibraryType.Should().Be(request.LibraryType);
        result.ContentLocations.Should().BeEquivalentTo(request.ContentLocations);
    }

    [Theory]
    [InlineData(LibraryType.Book)]
    [InlineData(LibraryType.Movie)]
    [InlineData(LibraryType.TvShow)]
    [InlineData(LibraryType.Music)]
    public void ToCommand_WhenMappingDifferentLibraryTypes_ShouldMapCorrectly(LibraryType libraryType)
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        AddLibraryRequest request = new(
            "My Library",
            libraryType,
            ["C:/Media"]
        );

        // Act
        AddLibraryCommand result = request.ToCommand(userId);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Title.Should().Be(request.Title);
        result.LibraryType.Should().Be(libraryType);
        result.ContentLocations.Should().BeEquivalentTo(request.ContentLocations);
    }

    [Fact]
    public void ToCommand_WhenMappingMultipleContentLocations_ShouldMapAllCorrectly()
    {
        // Arrange
        string[] contentLocations =
        [
            "C:/Media/Books",
            "D:/Books",
            "E:/Digital Library/Books",
            "F:/Reading Material"
        ];

        Guid userId = Guid.NewGuid();
        AddLibraryRequest request = new(
            "My Library",
            LibraryType.Book,
            contentLocations
        );

        // Act
        AddLibraryCommand result = request.ToCommand(userId);

        // Assert
        result.Should().NotBeNull();
        result.ContentLocations.Should().BeEquivalentTo(contentLocations);
    }
}
