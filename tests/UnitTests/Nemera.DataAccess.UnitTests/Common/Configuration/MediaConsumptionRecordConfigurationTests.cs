#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.DataAccess.UnitTests.Common.Configuration;

/// <summary>
/// Contains unit tests for the <see cref="MediaConsumptionRecordConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MediaConsumptionRecordConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaConsumptionRecordConfigurationTests"/> class.
    /// </summary>
    public MediaConsumptionRecordConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MediaConsumptionRecordEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("MediaConsumptionRecords", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureCascadingRelationshipWithUser()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MediaConsumptionRecordEntity))!;

        // Act
        IForeignKey userForeignKey = Assert.Single(entityType.GetForeignKeys());

        // Assert
        Assert.Equal(typeof(UserEntity), userForeignKey.PrincipalEntityType.ClrType);
        Assert.Equal(nameof(MediaConsumptionRecordEntity.UserId), userForeignKey.Properties[0].Name);
        Assert.Equal(DeleteBehavior.Cascade, userForeignKey.DeleteBehavior);
        Assert.True(userForeignKey.IsRequired);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldStoreTheKindAndStatusAsBoundedStrings()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MediaConsumptionRecordEntity))!;

        // Act
        IProperty mediaKindProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.MediaKind));
        IProperty statusProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.Status));

        // Assert
        Assert.Equal(typeof(MediaKind), mediaKindProperty.ClrType);
        Assert.Equal(typeof(string), mediaKindProperty.GetTypeMapping().Converter!.ProviderClrType);
        Assert.Equal("TEXT", mediaKindProperty.GetColumnType());
        Assert.Equal(20, mediaKindProperty.GetMaxLength());
        Assert.False(mediaKindProperty.IsNullable);

        Assert.Equal(typeof(ConsumptionStatus), statusProperty.ClrType);
        Assert.Equal(typeof(string), statusProperty.GetTypeMapping().Converter!.ProviderClrType);
        Assert.Equal("TEXT", statusProperty.GetColumnType());
        Assert.Equal(20, statusProperty.GetMaxLength());
        Assert.False(statusProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldStoreTheOptionalPositionKindAsABoundedString()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MediaConsumptionRecordEntity))!;

        // Act
        IProperty lastPositionKindProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.LastPositionKind));

        // Assert
        Assert.Equal(typeof(MediaPositionKind?), lastPositionKindProperty.ClrType);
        Assert.Equal(typeof(string), lastPositionKindProperty.GetTypeMapping().Converter!.ProviderClrType);
        Assert.Equal("TEXT", lastPositionKindProperty.GetColumnType());
        Assert.Equal(20, lastPositionKindProperty.GetMaxLength());
        Assert.True(lastPositionKindProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheRequiredColumns()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MediaConsumptionRecordEntity))!;

        // Act
        IProperty mediaKindProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.MediaKind));
        IProperty statusProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.Status));
        IProperty startedOnUtcProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.StartedOnUtc));
        IProperty lastActivityOnUtcProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.LastActivityOnUtc));
        IProperty consumedDurationMillisecondsProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.ConsumedDurationMilliseconds));
        IProperty lastProgressFractionProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.LastProgressFraction));
        IProperty consecutivePagesReadProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.ConsecutivePagesRead));
        IProperty versionProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.Version));
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.CreatedOnUtc));
        IProperty createdByProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.CreatedBy));

        // Assert
        Assert.False(mediaKindProperty.IsNullable);
        Assert.False(statusProperty.IsNullable);
        Assert.False(startedOnUtcProperty.IsNullable);
        Assert.False(lastActivityOnUtcProperty.IsNullable);
        Assert.False(consumedDurationMillisecondsProperty.IsNullable);
        Assert.False(lastProgressFractionProperty.IsNullable);
        Assert.False(consecutivePagesReadProperty.IsNullable);
        Assert.False(versionProperty.IsNullable);
        Assert.False(createdOnUtcProperty.IsNullable);
        Assert.False(createdByProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheOptionalColumns()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MediaConsumptionRecordEntity))!;

        // Act
        IProperty endedOnUtcProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.EndedOnUtc));
        IProperty lastPositionKindProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.LastPositionKind));
        IProperty lastPositionTimeOffsetMillisecondsProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.LastPositionTimeOffsetMilliseconds));
        IProperty lastPositionLocationProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.LastPositionLocation));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.UpdatedOnUtc));
        IProperty updatedByProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.UpdatedBy));

        // Assert
        Assert.True(endedOnUtcProperty.IsNullable);
        Assert.True(lastPositionKindProperty.IsNullable);
        Assert.True(lastPositionTimeOffsetMillisecondsProperty.IsNullable);
        Assert.True(lastPositionLocationProperty.IsNullable);
        Assert.True(updatedOnUtcProperty.IsNullable);
        Assert.True(updatedByProperty.IsNullable);
        Assert.Equal(4096, lastPositionLocationProperty.GetMaxLength());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheConsecutivePagesReadDefault()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MediaConsumptionRecordEntity))!;

        // Act
        IProperty consecutivePagesReadProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.ConsecutivePagesRead));

        // Assert
        Assert.Equal(0, consecutivePagesReadProperty.GetDefaultValue());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheVersionAsAConcurrencyToken()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MediaConsumptionRecordEntity))!;

        // Act
        IProperty versionProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.Version));

        // Assert
        Assert.True(versionProperty.IsConcurrencyToken);
        Assert.False(versionProperty.IsNullable);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureTheHistoryAndStatisticsIndexes()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MediaConsumptionRecordEntity))!;

        // Act
        IIndex historyIndex = Assert.Single(
            entityType.GetIndexes(),
            index => index.Properties.Select(property => property.Name).SequenceEqual(
            [
                nameof(MediaConsumptionRecordEntity.UserId),
                nameof(MediaConsumptionRecordEntity.MediaKind),
                nameof(MediaConsumptionRecordEntity.MediaItemId),
                nameof(MediaConsumptionRecordEntity.LastActivityOnUtc)
            ]));
        IIndex statisticsIndex = Assert.Single(
            entityType.GetIndexes(),
            index => index.Properties.Select(property => property.Name).SequenceEqual(
            [
                nameof(MediaConsumptionRecordEntity.UserId),
                nameof(MediaConsumptionRecordEntity.MediaKind),
                nameof(MediaConsumptionRecordEntity.StartedOnUtc)
            ]));

        // Assert
        Assert.False(historyIndex.IsUnique);
        Assert.False(statisticsIndex.IsUnique);
        // The two configured indexes are distinct, and neither of them is the foreign key index on the owner.
        Assert.NotSame(historyIndex, statisticsIndex);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureAuditProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(MediaConsumptionRecordEntity))!;

        // Act
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.CreatedOnUtc));
        IProperty createdByProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.CreatedBy));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.UpdatedOnUtc));
        IProperty updatedByProperty = entityType.GetProperty(nameof(MediaConsumptionRecordEntity.UpdatedBy));

        // Assert
        Assert.False(createdOnUtcProperty.IsNullable);
        Assert.False(createdByProperty.IsNullable);
        Assert.Null(updatedOnUtcProperty.GetDefaultValue());
        Assert.Null(updatedByProperty.GetDefaultValue());
    }
}
