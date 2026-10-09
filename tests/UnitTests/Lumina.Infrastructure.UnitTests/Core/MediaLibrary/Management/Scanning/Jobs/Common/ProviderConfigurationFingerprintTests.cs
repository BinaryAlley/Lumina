#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Plugins;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Infrastructure.Core.MediaLibrary.Management.Scanning.Jobs.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#endregion

namespace Lumina.Infrastructure.UnitTests.Core.MediaLibrary.Management.Scanning.Jobs.Common;

/// <summary>
/// Contains unit tests for the <see cref="ProviderConfigurationFingerprint"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class ProviderConfigurationFingerprintTests
{
    private readonly LibraryMetadataProviderConfigurationEntityFixture _libraryMetadataProviderConfigurationEntityFixture = new();
    private readonly LibraryArtworkProviderConfigurationEntityFixture _libraryArtworkProviderConfigurationEntityFixture = new();
    private readonly LibraryPathTemplatePartEntityFixture _libraryPathTemplatePartEntityFixture = new();

    [Fact]
    public void ComputeMetadataFingerprint_WhenCalledWithTheSameConfiguration_ShouldReturnTheSameFingerprint()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        List<LibraryMetadataProviderConfigurationEntity> configurations =
        [
            _libraryMetadataProviderConfigurationEntityFixture.Create(libraryId, Guid.NewGuid(), rank: 0),
            _libraryMetadataProviderConfigurationEntityFixture.Create(libraryId, Guid.NewGuid(), rank: 1)
        ];

        // Act
        string firstFingerprint = ProviderConfigurationFingerprint.ComputeMetadataFingerprint(configurations, false, true);
        string secondFingerprint = ProviderConfigurationFingerprint.ComputeMetadataFingerprint(configurations, false, true);

        // Assert
        Assert.Equal(firstFingerprint, secondFingerprint);
    }

    [Fact]
    public void ComputeMetadataFingerprint_WhenOnlyTheOrderOfConfigurationsDiffers_ShouldReturnTheSameFingerprint()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        LibraryMetadataProviderConfigurationEntity first = _libraryMetadataProviderConfigurationEntityFixture.Create(libraryId, Guid.NewGuid(), rank: 0);
        LibraryMetadataProviderConfigurationEntity second = _libraryMetadataProviderConfigurationEntityFixture.Create(libraryId, Guid.NewGuid(), rank: 1);

        // Act
        string firstFingerprint = ProviderConfigurationFingerprint.ComputeMetadataFingerprint([first, second], false, true);
        string secondFingerprint = ProviderConfigurationFingerprint.ComputeMetadataFingerprint([second, first], false, true);

        // Assert
        Assert.Equal(firstFingerprint, secondFingerprint);
    }

    [Fact]
    public void ComputeMetadataFingerprint_WhenAProviderChanges_ShouldReturnADifferentFingerprint()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        Guid pluginId = Guid.NewGuid();
        LibraryMetadataProviderConfigurationEntity enabled = _libraryMetadataProviderConfigurationEntityFixture.Create(libraryId, pluginId, rank: 0, isEnabled: true);
        LibraryMetadataProviderConfigurationEntity disabled = _libraryMetadataProviderConfigurationEntityFixture.Create(libraryId, pluginId, rank: 0, isEnabled: false);

        // Act
        string enabledFingerprint = ProviderConfigurationFingerprint.ComputeMetadataFingerprint([enabled], false, true);
        string disabledFingerprint = ProviderConfigurationFingerprint.ComputeMetadataFingerprint([disabled], false, true);

        // Assert
        Assert.NotEqual(enabledFingerprint, disabledFingerprint);
    }

    [Fact]
    public void ComputeMetadataFingerprint_WhenTheAggregationSettingChanges_ShouldReturnADifferentFingerprint()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        List<LibraryMetadataProviderConfigurationEntity> configurations = [_libraryMetadataProviderConfigurationEntityFixture.Create(libraryId, Guid.NewGuid(), rank: 0)];

        // Act
        string withoutAggregation = ProviderConfigurationFingerprint.ComputeMetadataFingerprint(configurations, false, true);
        string withAggregation = ProviderConfigurationFingerprint.ComputeMetadataFingerprint(configurations, true, true);

        // Assert
        Assert.NotEqual(withoutAggregation, withAggregation);
    }

    [Fact]
    public void ComputeMetadataFingerprint_WhenTheWebAccessSettingChanges_ShouldReturnADifferentFingerprint()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        List<LibraryMetadataProviderConfigurationEntity> configurations = [_libraryMetadataProviderConfigurationEntityFixture.Create(libraryId, Guid.NewGuid(), rank: 0)];

        // Act
        string withoutWebAccess = ProviderConfigurationFingerprint.ComputeMetadataFingerprint(configurations, false, false);
        string withWebAccess = ProviderConfigurationFingerprint.ComputeMetadataFingerprint(configurations, false, true);

        // Assert
        Assert.NotEqual(withoutWebAccess, withWebAccess);
    }

    [Fact]
    public void ComputeArtworkFingerprint_WhenOnlyTheOrderOfConfigurationsDiffers_ShouldReturnTheSameFingerprint()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        LibraryArtworkProviderConfigurationEntity first = _libraryArtworkProviderConfigurationEntityFixture.Create(libraryId, Guid.NewGuid(), rank: 0);
        LibraryArtworkProviderConfigurationEntity second = _libraryArtworkProviderConfigurationEntityFixture.Create(libraryId, Guid.NewGuid(), rank: 1);

        // Act
        string firstFingerprint = ProviderConfigurationFingerprint.ComputeArtworkFingerprint([first, second], false, true);
        string secondFingerprint = ProviderConfigurationFingerprint.ComputeArtworkFingerprint([second, first], false, true);

        // Assert
        Assert.Equal(firstFingerprint, secondFingerprint);
    }

    [Fact]
    public void ComputeArtworkFingerprint_WhenAProviderChanges_ShouldReturnADifferentFingerprint()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        LibraryArtworkProviderConfigurationEntity first = _libraryArtworkProviderConfigurationEntityFixture.Create(libraryId, Guid.NewGuid(), rank: 0);
        LibraryArtworkProviderConfigurationEntity second = _libraryArtworkProviderConfigurationEntityFixture.Create(libraryId, Guid.NewGuid(), rank: 0);

        // Act
        string firstFingerprint = ProviderConfigurationFingerprint.ComputeArtworkFingerprint([first], false, true);
        string secondFingerprint = ProviderConfigurationFingerprint.ComputeArtworkFingerprint([second], false, true);

        // Assert
        Assert.NotEqual(firstFingerprint, secondFingerprint);
    }

    [Fact]
    public void ComputeArtworkFingerprint_WhenTheAggregationSettingChanges_ShouldReturnADifferentFingerprint()
    {
        // Arrange
        Guid libraryId = Guid.NewGuid();
        List<LibraryArtworkProviderConfigurationEntity> configurations = [_libraryArtworkProviderConfigurationEntityFixture.Create(libraryId, Guid.NewGuid(), rank: 0)];

        // Act
        string withoutAggregation = ProviderConfigurationFingerprint.ComputeArtworkFingerprint(configurations, false, true);
        string withAggregation = ProviderConfigurationFingerprint.ComputeArtworkFingerprint(configurations, true, true);

        // Assert
        Assert.NotEqual(withoutAggregation, withAggregation);
    }

    [Fact]
    public void ComputePathTemplateFingerprint_WhenOnlyTheOrderOfPartsDiffers_ShouldReturnTheSameFingerprint()
    {
        // Arrange
        LibraryPathTemplatePartEntity first = _libraryPathTemplatePartEntityFixture.Create(position: 0, kind: LibraryPathPartKind.Artist, representation: "{0}");
        LibraryPathTemplatePartEntity second = _libraryPathTemplatePartEntityFixture.Create(position: 1, kind: LibraryPathPartKind.ReleaseName, representation: "{0}");

        // Act
        string firstFingerprint = ProviderConfigurationFingerprint.ComputePathTemplateFingerprint([first, second]);
        string secondFingerprint = ProviderConfigurationFingerprint.ComputePathTemplateFingerprint([second, first]);

        // Assert
        Assert.Equal(firstFingerprint, secondFingerprint);
    }

    [Fact]
    public void ComputePathTemplateFingerprint_WhenAPartChanges_ShouldReturnADifferentFingerprint()
    {
        // Arrange
        LibraryPathTemplatePartEntity artist = _libraryPathTemplatePartEntityFixture.Create(position: 0, kind: LibraryPathPartKind.Artist, representation: "{0}");
        LibraryPathTemplatePartEntity releaseName = _libraryPathTemplatePartEntityFixture.Create(position: 0, kind: LibraryPathPartKind.ReleaseName, representation: "{0}");

        // Act
        string artistFingerprint = ProviderConfigurationFingerprint.ComputePathTemplateFingerprint([artist]);
        string releaseNameFingerprint = ProviderConfigurationFingerprint.ComputePathTemplateFingerprint([releaseName]);

        // Assert
        Assert.NotEqual(artistFingerprint, releaseNameFingerprint);
    }

    [Fact]
    public void ComputePathTemplateFingerprint_WhenTheOptionalityChanges_ShouldReturnADifferentFingerprint()
    {
        // Arrange
        LibraryPathTemplatePartEntity required = _libraryPathTemplatePartEntityFixture.Create(position: 0, kind: LibraryPathPartKind.Artist, representation: "{0}", isOptional: false);
        LibraryPathTemplatePartEntity optional = _libraryPathTemplatePartEntityFixture.Create(position: 0, kind: LibraryPathPartKind.Artist, representation: "{0}", isOptional: true);

        // Act
        string requiredFingerprint = ProviderConfigurationFingerprint.ComputePathTemplateFingerprint([required]);
        string optionalFingerprint = ProviderConfigurationFingerprint.ComputePathTemplateFingerprint([optional]);

        // Assert
        Assert.NotEqual(requiredFingerprint, optionalFingerprint);
    }
}
