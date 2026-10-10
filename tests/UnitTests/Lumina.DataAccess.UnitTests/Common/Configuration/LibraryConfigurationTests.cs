#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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

    /// <summary>
    /// Gets the design-time model, which retains configuration such as column order that the read-optimized runtime model omits.
    /// </summary>
    private IModel DesignTimeModel => _context.GetService<IDesignTimeModel>().Model;

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
        IProperty downloadMetadataProperty = entityType.GetProperty(nameof(LibraryEntity.CanDownloadMetadataFromWeb));

        // Assert
        Assert.False(titleProperty.IsNullable);
        Assert.Equal(255, titleProperty.GetMaxLength());
        Assert.Equal(typeof(LibraryType), libraryTypeProperty.ClrType);
        Assert.Equal("TEXT", libraryTypeProperty.GetColumnType());
        Assert.Equal(true, isEnabledProperty.GetDefaultValue());
        Assert.Equal(true, downloadMetadataProperty.GetDefaultValue());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigurePathTemplateFingerprint()
    {
        // Arrange
        IEntityType entityType = DesignTimeModel.FindEntityType(typeof(LibraryEntity))!;

        // Act
        IProperty fingerprintProperty = entityType.GetProperty(nameof(LibraryEntity.PathTemplateFingerprint));

        // Assert
        Assert.True(fingerprintProperty.IsNullable);
        Assert.Equal(typeof(string), fingerprintProperty.ClrType);
        Assert.Equal(64, fingerprintProperty.GetMaxLength());
        Assert.Equal(11, fingerprintProperty.GetColumnOrder());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureOwnedContentLocations()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryEntity))!;

        // Act
        IEntityType contentLocationType = Assert.Single(
            _context.Model.GetEntityTypes(),
            ownedType => ownedType.IsOwned()
                && ownedType.ClrType == typeof(LibraryContentLocationEntity)
                && ownedType.FindOwnership()?.PrincipalEntityType.Name == entityType.Name);

        // Assert
        Assert.Equal(typeof(LibraryContentLocationEntity), contentLocationType.ClrType);
        Assert.Equal("LibraryContentLocations", contentLocationType.GetTableName());
        Assert.Equal(260, contentLocationType.GetProperty(nameof(LibraryContentLocationEntity.Path)).GetMaxLength());
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureOwnedPathTemplateParts()
    {
        // Arrange
        IEntityType entityType = _context.Model.FindEntityType(typeof(LibraryEntity))!;

        // Act
        IEntityType partEntityType = Assert.Single(
            _context.Model.GetEntityTypes(),
            ownedType => ownedType.IsOwned()
                && ownedType.ClrType == typeof(LibraryPathTemplatePartEntity)
                && ownedType.FindOwnership()?.PrincipalEntityType.Name == entityType.Name);
        IForeignKey ownershipForeignKey = partEntityType.FindOwnership()!;

        // Assert
        Assert.Equal("LibraryPathTemplateParts", partEntityType.GetTableName());
        Assert.Equal(["Id"], partEntityType.FindPrimaryKey()!.Properties.Select(property => property.Name));

        Assert.Equal(typeof(LibraryEntity), ownershipForeignKey.PrincipalEntityType.ClrType);
        Assert.Equal(["LibraryId"], ownershipForeignKey.Properties.Select(property => property.Name));
        Assert.True(ownershipForeignKey.IsRequired);

        IIndex uniqueIndex = Assert.Single(
            partEntityType.GetIndexes(),
            index => index.Properties.Select(property => property.Name).SequenceEqual(["LibraryId", nameof(LibraryPathTemplatePartEntity.Position)]));
        Assert.True(uniqueIndex.IsUnique);
    }

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureOwnedPathTemplatePartColumns()
    {
        // Arrange
        IEntityType partEntityType = DesignTimeModel.GetEntityTypes()
            .Single(entityType => entityType.IsOwned()
                && entityType.ClrType == typeof(LibraryPathTemplatePartEntity)
                && entityType.GetTableName() == "LibraryPathTemplateParts");

        // Act
        IProperty positionProperty = partEntityType.GetProperty(nameof(LibraryPathTemplatePartEntity.Position));
        IProperty kindProperty = partEntityType.GetProperty(nameof(LibraryPathTemplatePartEntity.Kind));
        IProperty representationProperty = partEntityType.GetProperty(nameof(LibraryPathTemplatePartEntity.Representation));
        IProperty isOptionalProperty = partEntityType.GetProperty(nameof(LibraryPathTemplatePartEntity.IsOptional));

        // Assert
        Assert.Equal("Position", positionProperty.GetColumnName());
        Assert.False(positionProperty.IsNullable);

        Assert.Equal(typeof(LibraryPathPartKind), kindProperty.ClrType);
        Assert.Equal(typeof(string), kindProperty.GetTypeMapping().Converter!.ProviderClrType);
        Assert.Equal("TEXT", kindProperty.GetColumnType());
        Assert.Equal(32, kindProperty.GetMaxLength());
        Assert.False(kindProperty.IsNullable);

        Assert.Equal(512, representationProperty.GetMaxLength());
        Assert.False(representationProperty.IsNullable);

        Assert.Equal(typeof(bool), isOptionalProperty.ClrType);
        Assert.False(isOptionalProperty.IsNullable);
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

    [Fact]
    public void Configure_WhenApplied_ShouldConfigureAuditColumnOrders()
    {
        // Arrange
        IEntityType entityType = DesignTimeModel.FindEntityType(typeof(LibraryEntity))!;

        // Act
        IProperty createdOnUtcProperty = entityType.GetProperty(nameof(LibraryEntity.CreatedOnUtc));
        IProperty createdByProperty = entityType.GetProperty(nameof(LibraryEntity.CreatedBy));
        IProperty updatedOnUtcProperty = entityType.GetProperty(nameof(LibraryEntity.UpdatedOnUtc));
        IProperty updatedByProperty = entityType.GetProperty(nameof(LibraryEntity.UpdatedBy));

        // Assert
        Assert.Equal(12, createdOnUtcProperty.GetColumnOrder());
        Assert.Equal(13, createdByProperty.GetColumnOrder());
        Assert.Equal(14, updatedOnUtcProperty.GetColumnOrder());
        Assert.Equal(15, updatedByProperty.GetColumnOrder());
    }
}
