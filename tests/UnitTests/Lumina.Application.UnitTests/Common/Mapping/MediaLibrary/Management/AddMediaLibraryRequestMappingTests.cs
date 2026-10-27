#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Commands.AddLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="AddMediaLibraryRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddMediaLibraryRequestMappingTests
{
    private readonly AddLibraryRequestFixture _addLibraryRequestFixture = new();
    private readonly LibraryPathTemplatePartRequestFixture _libraryPathTemplatePartRequestFixture = new();

    [Fact]
    public void ToCommand_WhenMappingValidRequest_ShouldMapCorrectly()
    {
        // Arrange
        AddLibraryRequest request = _addLibraryRequestFixture.Create(
            title: "My Library",
            libraryType: "Book",
            contentLocations: ["C:/Books", "D:/Media/Books"],
            coverImage: "D:/poster.jpg",
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: false,
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 90
        );

        // Act
        AddLibraryCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.LibraryType, result.LibraryType);
        Assert.Equal(request.ContentLocations, result.ContentLocations);
        Assert.Equal(request.CoverImage, result.CoverImage);
        Assert.Equal(request.IsEnabled, result.IsEnabled);
        Assert.Equal(request.IsLocked, result.IsLocked);
        Assert.Equal(request.CanDownloadMetadataFromWeb, result.CanDownloadMetadataFromWeb);
        Assert.Equal(request.ShouldSaveMetadataInMediaDirectories, result.ShouldSaveMetadataInMediaDirectories);
        Assert.Equal(request.ShouldTrackConsumptionProgress, result.ShouldTrackConsumptionProgress);
        Assert.Equal(request.CompletionThresholdPercentage, result.CompletionThresholdPercentage);
    }

    [Theory]
    [InlineData(LibraryType.Book)]
    [InlineData(LibraryType.Movie)]
    [InlineData(LibraryType.TvShow)]
    [InlineData(LibraryType.Music)]
    public void ToCommand_WhenMappingDifferentLibraryTypes_ShouldMapCorrectly(LibraryType libraryType)
    {
        // Arrange
        AddLibraryRequest request = _addLibraryRequestFixture.Create(
            title: "My Library",
            libraryType: libraryType.ToString(),
            contentLocations: ["C:/Media"],
            coverImage: "D:/poster.jpg",
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: false
        );

        // Act
        AddLibraryCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(libraryType.ToString(), result.LibraryType);
        Assert.Equal(request.ContentLocations, result.ContentLocations);
        Assert.Equal(request.CoverImage, result.CoverImage);
        Assert.Equal(request.IsEnabled, result.IsEnabled);
        Assert.Equal(request.IsLocked, result.IsLocked);
        Assert.Equal(request.CanDownloadMetadataFromWeb, result.CanDownloadMetadataFromWeb);
        Assert.Equal(request.ShouldSaveMetadataInMediaDirectories, result.ShouldSaveMetadataInMediaDirectories);
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

        AddLibraryRequest request = _addLibraryRequestFixture.Create(
            title: "My Library",
            libraryType: "Book",
            contentLocations: contentLocations,
            coverImage: "D:/poster.jpg",
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: false
        );

        // Act
        AddLibraryCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(contentLocations, result.ContentLocations);
    }

    [Fact]
    public void ToCommand_WhenMappingWithNullCover_ShouldMapCorrectly()
    {
        // Arrange
        string[] contentLocations =
        [
            "C:/Media/Books",
            "D:/Books"
        ];

        AddLibraryRequest request = _addLibraryRequestFixture.Create(
            title: "My Library",
            libraryType: "Book",
            contentLocations: contentLocations,
            coverImage: null,
            isEnabled: true,
            isLocked: false,
            canDownloadMetadataFromWeb: true,
            shouldSaveMetadataInMediaDirectories: false,
            shouldSkipUnchangedDirectoriesDuringScan: false
        );

        // Act
        AddLibraryCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.CoverImage);
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
        AddLibraryRequest request = _addLibraryRequestFixture.Create(pathTemplateParts: [literalPart, typedPart]);

        // Act
        AddLibraryCommand result = request.ToCommand();

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
        AddLibraryRequest request = _addLibraryRequestFixture.Create(pathTemplateParts: null);

        // Act
        AddLibraryCommand result = request.ToCommand();

        // Assert
        Assert.Null(result.PathTemplateParts);
    }

    [Fact]
    public void ToCommand_WhenConsumptionSettingsAreSupplied_ShouldMapThem()
    {
        // Arrange
        AddLibraryRequest request = _addLibraryRequestFixture.Create(
            libraryType: "Book",
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 75);

        // Act
        AddLibraryCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result.ShouldTrackConsumptionProgress);
        Assert.True(result.ShouldTrackConsumptionProgress.Value);
        Assert.Equal(75, result.CompletionThresholdPercentage);
    }

    [Fact]
    public void ToCommand_WhenConsumptionSettingsAreAbsent_ShouldPreserveThemAsNull()
    {
        // Arrange
        AddLibraryRequest request = _addLibraryRequestFixture.Create(
            libraryType: "Book",
            shouldTrackConsumptionProgress: null,
            completionThresholdPercentage: null);

        // Act
        AddLibraryCommand result = request.ToCommand();

        // Assert
        Assert.Null(result.ShouldTrackConsumptionProgress);
        Assert.Null(result.CompletionThresholdPercentage);
    }

    [Fact]
    public void ToCommand_WhenProgressTrackingIsExplicitlyDisabled_ShouldPreserveTheDisabledFlagWithoutThreshold()
    {
        // Arrange
        AddLibraryRequest request = _addLibraryRequestFixture.Create(
            libraryType: "Book",
            shouldTrackConsumptionProgress: false,
            completionThresholdPercentage: null);

        // Act
        AddLibraryCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result.ShouldTrackConsumptionProgress);
        Assert.False(result.ShouldTrackConsumptionProgress.Value);
        Assert.Null(result.CompletionThresholdPercentage);
    }
}
