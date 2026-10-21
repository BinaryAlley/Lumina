#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Configuration;

/// <summary>
/// Contains unit tests for the <see cref="UserSettingsConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class UserSettingsConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserSettingsConfigurationTests"/> class.
    /// </summary>
    public UserSettingsConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(UserSettingsEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("UserSettings", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureScalarPropertiesAsRequired()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(UserSettingsEntity))!;

        // Act
        IProperty userIdProperty = entityType.GetProperty(nameof(UserSettingsEntity.UserId));
        IProperty isPaginationEnabledProperty = entityType.GetProperty(nameof(UserSettingsEntity.IsPaginationEnabled));
        IProperty itemsPerPageProperty = entityType.GetProperty(nameof(UserSettingsEntity.ItemsPerPage));
        IProperty ignoreThePrefixProperty = entityType.GetProperty(nameof(UserSettingsEntity.ShouldIgnoreThePrefixForAlphaPicker));
        IProperty vuMeterOrientationProperty = entityType.GetProperty(nameof(UserSettingsEntity.VuMeterOrientation));
        IProperty vuMeterSegmentCountProperty = entityType.GetProperty(nameof(UserSettingsEntity.VuMeterSegmentCount));
        IProperty vuMeterSegmentGapProperty = entityType.GetProperty(nameof(UserSettingsEntity.VuMeterSegmentGap));
        IProperty vuMeterMediumThresholdProperty = entityType.GetProperty(nameof(UserSettingsEntity.VuMeterMediumThreshold));
        IProperty vuMeterHighThresholdProperty = entityType.GetProperty(nameof(UserSettingsEntity.VuMeterHighThreshold));
        IProperty vuMeterShowPeakProperty = entityType.GetProperty(nameof(UserSettingsEntity.VuMeterShowPeak));
        IProperty vuMeterPeakHoldDurationProperty = entityType.GetProperty(nameof(UserSettingsEntity.VuMeterPeakHoldDurationInMilliseconds));
        IProperty vuMeterReleasePerSecondProperty = entityType.GetProperty(nameof(UserSettingsEntity.VuMeterReleasePerSecond));
        IProperty vuMeterPeakReleasePerSecondProperty = entityType.GetProperty(nameof(UserSettingsEntity.VuMeterPeakReleasePerSecond));

        // Assert
        Assert.False(userIdProperty.IsNullable);
        Assert.False(isPaginationEnabledProperty.IsNullable);
        Assert.False(itemsPerPageProperty.IsNullable);
        Assert.False(ignoreThePrefixProperty.IsNullable);
        Assert.False(vuMeterOrientationProperty.IsNullable);
        Assert.False(vuMeterSegmentCountProperty.IsNullable);
        Assert.False(vuMeterSegmentGapProperty.IsNullable);
        Assert.False(vuMeterMediumThresholdProperty.IsNullable);
        Assert.False(vuMeterHighThresholdProperty.IsNullable);
        Assert.False(vuMeterShowPeakProperty.IsNullable);
        Assert.False(vuMeterPeakHoldDurationProperty.IsNullable);
        Assert.False(vuMeterReleasePerSecondProperty.IsNullable);
        Assert.False(vuMeterPeakReleasePerSecondProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureOneToOneRelationshipWithUser()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(UserSettingsEntity))!;

        // Act
        IForeignKey foreignKey = Assert.Single(entityType.GetForeignKeys());

        // Assert
        Assert.Equal(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
        Assert.Equal(typeof(UserEntity), foreignKey.PrincipalEntityType.ClrType);
        Assert.Equal(nameof(UserSettingsEntity.UserId), foreignKey.Properties[0].Name);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureUniqueIndexOnUserId()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(UserSettingsEntity))!;

        // Act
        IIndex userIdIndex = Assert.Single(entityType.GetIndexes());

        // Assert
        Assert.True(userIdIndex.IsUnique);
        Assert.Equal(nameof(UserSettingsEntity.UserId), userIdIndex.Properties[0].Name);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureAuditProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(UserSettingsEntity))!;

        // Act
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(UserSettingsEntity.CreatedOnUtc));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(UserSettingsEntity.UpdatedOnUtc));

        // Assert
        Assert.False(createdOnUtcProperty.IsNullable);
        Assert.Null(updatedOnUtcProperty.GetDefaultValue());
    }
}
