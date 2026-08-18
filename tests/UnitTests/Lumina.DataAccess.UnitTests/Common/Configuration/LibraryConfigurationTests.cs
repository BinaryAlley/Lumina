#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.DataAccess.Core.UoW;
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
/// Contains unit tests for the <see cref="LibraryConfiguration"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryConfigurationTests
{
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="LibraryConfigurationTests"/> class.
    /// </summary>
    public LibraryConfigurationTests()
    {
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite("Data Source=:memory:").Options);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldSetTableNameAndKey()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryEntity))!;

        // Act
        string? tableName = entityType.GetTableName();
        IReadOnlyList<IProperty> keyProperties = entityType.FindPrimaryKey()!.Properties;

        // Assert
        Assert.Equal("Libraries", tableName);
        Assert.Equal(["Id"], keyProperties.Select(property => property.Name));
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureScalarProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryEntity))!;

        // Act
        IProperty titleProperty = entityType.GetProperty(nameof(LibraryEntity.Title));
        IProperty libraryTypeProperty = entityType.GetProperty(nameof(LibraryEntity.LibraryType));
        IProperty isEnabledProperty = entityType.GetProperty(nameof(LibraryEntity.IsEnabled));
        IProperty downloadMetadataProperty = entityType.GetProperty(nameof(LibraryEntity.DownloadMetadataFromWeb));

        // Assert
        Assert.False(titleProperty.IsNullable);
        Assert.Equal(255, titleProperty.GetMaxLength());
        Assert.Equal(typeof(LibraryType), libraryTypeProperty.ClrType);
        Assert.Equal("TEXT", libraryTypeProperty.GetColumnType());
        Assert.Equal(true, isEnabledProperty.GetDefaultValue());
        Assert.Equal(true, downloadMetadataProperty.GetDefaultValue());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureOwnedContentLocations()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryEntity))!;

        // Act
        List<IEntityType> ownedTypes = [.. _context.Model.GetEntityTypes().Where(ownedType => ownedType.IsOwned() && ownedType.FindOwnership()?.PrincipalEntityType.Name == entityType.Name)];

        // Assert
        IEntityType contentLocationType = Assert.Single(ownedTypes);
        Assert.Equal(typeof(LibraryContentLocationEntity), contentLocationType.ClrType);
        Assert.Equal("LibraryContentLocations", contentLocationType.GetTableName());
        Assert.Equal(260, contentLocationType.GetProperty(nameof(LibraryContentLocationEntity.Path)).GetMaxLength());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureRelationships()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryEntity))!;

        // Act
        IForeignKey userForeignKey = Assert.Single(entityType.GetForeignKeys());
        List<INavigation> navigations = [.. entityType.GetNavigations()];

        // Assert
        Assert.Equal(DeleteBehavior.Cascade, userForeignKey.DeleteBehavior);
        Assert.Equal(nameof(LibraryEntity.UserId), userForeignKey.Properties[0].Name);
        Assert.Contains(navigations, navigation => navigation.Name == nameof(LibraryEntity.User));
        Assert.Contains(navigations, navigation => navigation.Name == nameof(LibraryEntity.LibraryScans) && navigation.IsCollection);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureAuditProperties()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryEntity))!;

        // Act
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(LibraryEntity.CreatedOnUtc));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(LibraryEntity.UpdatedOnUtc));

        // Assert
        Assert.False(createdOnUtcProperty.IsNullable);
        Assert.Null(updatedOnUtcProperty.GetDefaultValue());
    }
}
