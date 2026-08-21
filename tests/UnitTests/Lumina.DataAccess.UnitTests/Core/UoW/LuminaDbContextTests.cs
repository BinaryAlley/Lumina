#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.UnitTests.Core.UoW;

/// <summary>
/// Contains unit tests for the <see cref="LuminaDbContext"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LuminaDbContextTests : IDisposable
{
    private readonly string _connectionString;
    private readonly SqliteConnection _anchorConnection;
    private readonly LuminaDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="LuminaDbContextTests"/> class.
    /// </summary>
    public LuminaDbContextTests()
    {
        _connectionString = $"Data Source=luminadataccess-dbcontext-tests-{Guid.NewGuid()};Mode=Memory;Cache=Shared";
        _anchorConnection = new SqliteConnection(_connectionString);
        _anchorConnection.Open();
        _context = new LuminaDbContext(new DbContextOptionsBuilder<LuminaDbContext>().UseSqlite(_connectionString).Options);
    }

    [Fact]
    public void OnModelCreating_WhenCalled_ShouldApplyAllEntityConfigurationsFromTheAssembly()
    {
        // Arrange
        IModel model = _context.Model;

        // Act
        string[] tableNames = [.. model.GetEntityTypes().Select(entityType => entityType.GetTableName()).OfType<string>().OrderBy(name => name)];

        // Assert
        // The count assertion fails when a table is added or removed, reminding to update the assertions below.
        Assert.Equal(24, tableNames.Length);
        Assert.Contains("Books", tableNames);
        Assert.Contains("Users", tableNames);
        Assert.Contains("UserSettings", tableNames);
        Assert.Contains("Roles", tableNames);
        Assert.Contains("Permissions", tableNames);
        Assert.Contains("UserRoles", tableNames);
        Assert.Contains("UserPermissions", tableNames);
        Assert.Contains("RolePermissions", tableNames);
        Assert.Contains("Libraries", tableNames);
        Assert.Contains("LibraryScans", tableNames);
        Assert.Contains("LibraryScanResults", tableNames);
        Assert.Contains("LibraryScanSnapshots", tableNames);
        Assert.Contains("LibraryScanStagingResults", tableNames);
        Assert.Contains("DirectoryScanFingerprints", tableNames);
        Assert.Contains("Plugins", tableNames);
        Assert.Contains("LibraryMetadataProviderConfigurations", tableNames);
        Assert.Contains("Tags", tableNames);
        Assert.Contains("Genres", tableNames);
        Assert.Contains("LibraryContentLocations", tableNames);
        Assert.Contains("BookTags", tableNames);
        Assert.Contains("BookGenres", tableNames);
        Assert.Contains("BookRatings", tableNames);
        Assert.Contains("BookISBNs", tableNames);
        Assert.Contains("Themes", tableNames);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenAddingBook_ShouldPersistItToTheDatabase()
    {
        // Arrange
        _context.Database.EnsureCreated();
        BookEntity book = new()
        {
            Id = Guid.NewGuid(),
            LibraryId = Guid.NewGuid(),
            Path = "/books/test.epub",
            Title = "Test Book",
            MetadataStatus = MetadataStatus.Pending,
            CreatedOnUtc = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = null
        };
        _context.Books.Add(book);

        // Act
        await _context.SaveChangesAsync();

        // Assert
        BookEntity? retrievedBook = await _context.Books.FindAsync(book.Id);
        Assert.NotNull(retrievedBook);
        Assert.Equal("Test Book", retrievedBook!.Title);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _context.Dispose();
        _anchorConnection.Dispose();
    }
}
