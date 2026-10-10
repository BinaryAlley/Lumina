#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Configuration;

/// <summary>
/// Contains unit tests for the <see cref="MusicLibraryScanItemMetadataConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicLibraryScanItemMetadataConfigurationTests : IDisposable
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="MusicLibraryScanItemMetadataConfigurationTests"/> class.
    /// </summary>
    public MusicLibraryScanItemMetadataConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MusicLibraryScanItemMetadataEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("MusicLibraryScanItemMetadata", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldNotGenerateTheIdValue()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MusicLibraryScanItemMetadataEntity))!;

        // Act
        IProperty idProperty = entityType.GetProperty(nameof(MusicLibraryScanItemMetadataEntity.Id));

        // Assert
        Assert.Equal(ValueGenerated.Never, idProperty.ValueGenerated);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureRequiredScalarProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MusicLibraryScanItemMetadataEntity))!;

        // Act
        IProperty libraryScanIdProperty = entityType.GetProperty(nameof(MusicLibraryScanItemMetadataEntity.LibraryScanId));
        IProperty libraryIdProperty = entityType.GetProperty(nameof(MusicLibraryScanItemMetadataEntity.LibraryId));
        IProperty pathProperty = entityType.GetProperty(nameof(MusicLibraryScanItemMetadataEntity.Path));

        // Assert
        Assert.False(libraryScanIdProperty.IsNullable);
        Assert.False(libraryIdProperty.IsNullable);
        Assert.False(pathProperty.IsNullable);
        Assert.Equal(2048, pathProperty.GetMaxLength());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureOptionalScalarProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MusicLibraryScanItemMetadataEntity))!;

        // Act
        IProperty artistNameProperty = entityType.GetProperty(nameof(MusicLibraryScanItemMetadataEntity.ArtistName));
        IProperty releaseTypeProperty = entityType.GetProperty(nameof(MusicLibraryScanItemMetadataEntity.ReleaseType));
        IProperty releaseYearProperty = entityType.GetProperty(nameof(MusicLibraryScanItemMetadataEntity.ReleaseYear));
        IProperty releaseNameProperty = entityType.GetProperty(nameof(MusicLibraryScanItemMetadataEntity.ReleaseName));
        IProperty trackTitleProperty = entityType.GetProperty(nameof(MusicLibraryScanItemMetadataEntity.TrackTitle));
        IProperty audioCodecProperty = entityType.GetProperty(nameof(MusicLibraryScanItemMetadataEntity.AudioCodec));
        IProperty workTitleProperty = entityType.GetProperty(nameof(MusicLibraryScanItemMetadataEntity.WorkTitle));
        IProperty acoustIdProperty = entityType.GetProperty(nameof(MusicLibraryScanItemMetadataEntity.AcoustId));

        // Assert
        Assert.True(artistNameProperty.IsNullable);
        Assert.Equal(255, artistNameProperty.GetMaxLength());
        Assert.True(releaseTypeProperty.IsNullable);
        Assert.Equal(typeof(MusicReleaseType?), releaseTypeProperty.ClrType);
        Assert.Equal("TEXT", releaseTypeProperty.GetColumnType());
        Assert.Equal(50, releaseTypeProperty.GetMaxLength());
        Assert.True(releaseYearProperty.IsNullable);
        Assert.Equal(255, releaseNameProperty.GetMaxLength());
        Assert.Equal(255, trackTitleProperty.GetMaxLength());
        Assert.Equal(50, audioCodecProperty.GetMaxLength());
        Assert.Equal(255, workTitleProperty.GetMaxLength());
        Assert.Equal(64, acoustIdProperty.GetMaxLength());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureReplayGainDecimalPrecision()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MusicLibraryScanItemMetadataEntity))!;

        // Act
        IProperty trackGainProperty = entityType.GetProperty(nameof(MusicLibraryScanItemMetadataEntity.ReplayGainTrackGain));
        IProperty trackPeakProperty = entityType.GetProperty(nameof(MusicLibraryScanItemMetadataEntity.ReplayGainTrackPeak));
        IProperty albumGainProperty = entityType.GetProperty(nameof(MusicLibraryScanItemMetadataEntity.ReplayGainAlbumGain));
        IProperty albumPeakProperty = entityType.GetProperty(nameof(MusicLibraryScanItemMetadataEntity.ReplayGainAlbumPeak));

        // Assert
        Assert.Equal("decimal(8,2)", trackGainProperty.GetColumnType());
        Assert.Equal("decimal(8,6)", trackPeakProperty.GetColumnType());
        Assert.Equal("decimal(8,2)", albumGainProperty.GetColumnType());
        Assert.Equal("decimal(8,6)", albumPeakProperty.GetColumnType());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureMoodsAsPrimitiveCollection()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MusicLibraryScanItemMetadataEntity))!;

        // Act
        IProperty moodsProperty = entityType.GetProperty(nameof(MusicLibraryScanItemMetadataEntity.Moods));

        // Assert
        Assert.True(moodsProperty.IsPrimitiveCollection);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureUniqueScanIdPathIndex()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MusicLibraryScanItemMetadataEntity))!;

        // Act
        List<IIndex> indexes = [.. entityType.GetIndexes()];

        // Assert
        IIndex uniqueIndex = Assert.Single(indexes, index => index.IsUnique);
        Assert.Equal(
            [nameof(MusicLibraryScanItemMetadataEntity.LibraryScanId), nameof(MusicLibraryScanItemMetadataEntity.Path)],
            uniqueIndex.Properties.Select(property => property.Name));
    }

    /// <summary>
    /// Disposes the database context.
    /// </summary>
    public void Dispose()
    {
        _context.Dispose();
    }
}
