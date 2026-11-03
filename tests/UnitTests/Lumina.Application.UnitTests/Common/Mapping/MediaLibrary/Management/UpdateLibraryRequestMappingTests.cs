#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DTO.MediaLibrary.Management;
using Lumina.Application.Common.Mapping.MediaLibrary.Management;
using Lumina.Application.Core.MediaLibrary.Management.Commands.UpdateLibrary;
using Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.UnitTests.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Contains unit tests for the <see cref="UpdateLibraryRequestMapping"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateLibraryRequestMappingTests
{
    private readonly UpdateLibraryRequestFixture _updateLibraryRequestFixture = new();
    private readonly LibraryProviderSelectionRequestFixture _libraryProviderSelectionRequestFixture = new();
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
            shouldSkipUnchangedDirectoriesDuringScan: false,
            isContentOnSsd: true,
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 90);

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
        Assert.Equal(request.IsContentOnSsd, result.IsContentOnSsd);
        Assert.Equal(request.ShouldTrackConsumptionProgress, result.ShouldTrackConsumptionProgress);
        Assert.Equal(request.CompletionThresholdPercentage, result.CompletionThresholdPercentage);
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

    [Fact]
    public void ToCommand_WhenConsumptionSettingsAreSupplied_ShouldMapThem()
    {
        // Arrange
        UpdateLibraryRequest request = _updateLibraryRequestFixture.Create(
            libraryType: "Book",
            shouldTrackConsumptionProgress: true,
            completionThresholdPercentage: 75);

        // Act
        UpdateLibraryCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result.ShouldTrackConsumptionProgress);
        Assert.True(result.ShouldTrackConsumptionProgress.Value);
        Assert.Equal(75, result.CompletionThresholdPercentage);
    }

    [Fact]
    public void ToCommand_WhenConsumptionSettingsAreAbsent_ShouldPreserveThemAsNull()
    {
        // Arrange
        UpdateLibraryRequest request = _updateLibraryRequestFixture.Create(
            libraryType: "Book",
            shouldTrackConsumptionProgress: null,
            completionThresholdPercentage: null);

        // Act
        UpdateLibraryCommand result = request.ToCommand();

        // Assert
        Assert.Null(result.ShouldTrackConsumptionProgress);
        Assert.Null(result.CompletionThresholdPercentage);
    }

    [Fact]
    public void ToCommand_WhenProgressTrackingIsExplicitlyDisabled_ShouldPreserveTheDisabledFlagWithoutThreshold()
    {
        // Arrange
        UpdateLibraryRequest request = _updateLibraryRequestFixture.Create(
            libraryType: "Book",
            shouldTrackConsumptionProgress: false,
            completionThresholdPercentage: null);

        // Act
        UpdateLibraryCommand result = request.ToCommand();

        // Assert
        Assert.NotNull(result.ShouldTrackConsumptionProgress);
        Assert.False(result.ShouldTrackConsumptionProgress.Value);
        Assert.Null(result.CompletionThresholdPercentage);
    }

    [Fact]
    public void ToCommand_WhenProviderChoicesAreProvided_ShouldMapThemInOrder()
    {
        // Arrange
        LibraryProviderSelectionRequest[] metadataProviders = [_libraryProviderSelectionRequestFixture.Create(isEnabled: true), _libraryProviderSelectionRequestFixture.Create(isEnabled: false)];
        LibraryProviderSelectionRequest[] artworkProviders = [_libraryProviderSelectionRequestFixture.Create(isEnabled: false)];
        LibraryProviderSelectionRequest[] bookReaders = [_libraryProviderSelectionRequestFixture.Create(isEnabled: true)];
        UpdateLibraryRequest request = _updateLibraryRequestFixture.Create(metadataProviders: metadataProviders, artworkProviders: artworkProviders, bookReaders: bookReaders);

        // Act
        UpdateLibraryCommand result = request.ToCommand();

        // Assert
        Assert.Equal(metadataProviders.Select(selection => (selection.PluginId, selection.IsEnabled)), result.MetadataProviders!.Select(selection => (selection.PluginId, selection.IsEnabled)));
        Assert.Equal(artworkProviders.Select(selection => (selection.PluginId, selection.IsEnabled)), result.ArtworkProviders!.Select(selection => (selection.PluginId, selection.IsEnabled)));
        Assert.Equal(bookReaders.Select(selection => (selection.PluginId, selection.IsEnabled)), result.BookReaders!.Select(selection => (selection.PluginId, selection.IsEnabled)));
    }

    [Fact]
    public void ToCommand_WhenProviderChoicesAreNotProvided_ShouldMapThemAsNull()
    {
        // Arrange
        UpdateLibraryRequest request = _updateLibraryRequestFixture.Create(metadataProviders: null, artworkProviders: null, bookReaders: null);

        // Act
        UpdateLibraryCommand result = request.ToCommand();

        // Assert
        Assert.Null(result.MetadataProviders);
        Assert.Null(result.ArtworkProviders);
        Assert.Null(result.BookReaders);
    }

    [Fact]
    public void ToCommand_WhenAProviderChoiceIsNull_ShouldMapItToAChoiceWithoutPluginId()
    {
        // Arrange
        UpdateLibraryRequest request = _updateLibraryRequestFixture.Create(metadataProviders: [null], artworkProviders: [null], bookReaders: [null]);

        // Act
        UpdateLibraryCommand result = request.ToCommand();

        // Assert
        Assert.Equal(Guid.Empty, Assert.Single(result.MetadataProviders!).PluginId);
        Assert.Null(result.MetadataProviders![0].IsEnabled);
        Assert.Equal(Guid.Empty, Assert.Single(result.ArtworkProviders!).PluginId);
        Assert.Equal(Guid.Empty, Assert.Single(result.BookReaders!).PluginId);
    }

    [Fact]
    public void ToCommand_WhenAPathTemplatePartIsNull_ShouldMapItToAPartWithoutKind()
    {
        // Arrange
        UpdateLibraryRequest request = _updateLibraryRequestFixture.Create(pathTemplateParts: [null]);

        // Act
        UpdateLibraryCommand result = request.ToCommand();

        // Assert
        LibraryPathTemplatePartDto part = Assert.Single(result.PathTemplateParts!);
        Assert.Null(part.Kind);
        Assert.Null(part.Representation);
        Assert.False(part.IsOptional);
    }

    [Fact]
    public void ToCommand_WhenAProviderChoiceHasNoEnabledState_ShouldMapTheStateAsNull()
    {
        // Arrange
        LibraryProviderSelectionRequest choice = _libraryProviderSelectionRequestFixture.Create(includeIsEnabled: false);
        UpdateLibraryRequest request = _updateLibraryRequestFixture.Create(metadataProviders: [choice]);

        // Act
        UpdateLibraryCommand result = request.ToCommand();

        // Assert
        LibraryProviderSelectionDto mappedChoice = Assert.Single(result.MetadataProviders!);
        Assert.Equal(choice.PluginId, mappedChoice.PluginId);
        Assert.Null(mappedChoice.IsEnabled);
    }
}
