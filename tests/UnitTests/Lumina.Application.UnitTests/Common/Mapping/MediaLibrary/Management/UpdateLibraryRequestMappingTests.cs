#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Commands.UpdateLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="UpdateLibraryRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateLibraryRequestMappingTests
{
    private readonly UpdateLibraryRequestFixture _updateLibraryRequestFixture = new();
    private readonly LibraryPathTemplatePartRequestFixture _libraryPathTemplatePartRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        UpdateLibraryRequest request = _updateLibraryRequestFixture.Create(
            title: "My Library",
            libraryType: "Book",
            contentLocations: ["C:/Books", "D:/Media/Books"],
            coverImage: "D:/poster.jpg",
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: false);

        // Act
        UpdateLibraryCommand result = request.ToCommand();

        // Assert
        Assert.Equal(request.Id, result.Id);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.LibraryType, result.LibraryType);
        Assert.Equal(request.ContentLocations, result.ContentLocations);
        Assert.Equal(request.CoverImage, result.CoverImage);
        Assert.Equal(request.IsEnabled, result.IsEnabled);
        Assert.Equal(request.IsLocked, result.IsLocked);
        Assert.Equal(request.CanDownloadMetadataFromWeb, result.CanDownloadMetadataFromWeb);
        Assert.Equal(request.ShouldSaveMetadataInMediaDirectories, result.ShouldSaveMetadataInMediaDirectories);
        Assert.Equal(request.ShouldSkipUnchangedDirectoriesDuringScan, result.ShouldSkipUnchangedDirectoriesDuringScan);
    }

    [Fact]
    public void ToCommand_WhenRequestHasPathTemplateParts_ShouldMapToDtoArray()
    {
        // Arrange
        LibraryPathTemplatePartRequest literalPart = _libraryPathTemplatePartRequestFixture.Create(
            kind: nameof(LibraryPathPartKind.Literal),
            representation: "Various Artists",
            isOptional: false);
        LibraryPathTemplatePartRequest typedPart = _libraryPathTemplatePartRequestFixture.Create(
            kind: nameof(LibraryPathPartKind.Artist),
            representation: "{0}",
            isOptional: true);
        UpdateLibraryRequest request = _updateLibraryRequestFixture.Create(pathTemplateParts: [literalPart, typedPart]);

        // Act
        UpdateLibraryCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result.PathTemplateParts);
        Assert.Equal(2, result.PathTemplateParts.Length);
        Assert.Equal(literalPart.Kind, result.PathTemplateParts[0].Kind);
        Assert.Equal(literalPart.Representation, result.PathTemplateParts[0].Representation);
        Assert.Equal(literalPart.IsOptional, result.PathTemplateParts[0].IsOptional);
        Assert.Equal(typedPart.Kind, result.PathTemplateParts[1].Kind);
        Assert.Equal(typedPart.Representation, result.PathTemplateParts[1].Representation);
        Assert.Equal(typedPart.IsOptional, result.PathTemplateParts[1].IsOptional);
    }

    [Fact]
    public void ToCommand_WhenPathTemplatePartsIsNull_ShouldMapToNull()
    {
        // Arrange
        UpdateLibraryRequest request = _updateLibraryRequestFixture.Create(pathTemplateParts: null);

        // Act
        UpdateLibraryCommand result = request.ToCommand();

        // Assert
        Assert.Null(result.PathTemplateParts);
    }
}
