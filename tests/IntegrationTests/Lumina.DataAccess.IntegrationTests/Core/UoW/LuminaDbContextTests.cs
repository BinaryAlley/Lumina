#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.DataAccess.Core.UoW;
using Lumina.Domain.SharedKernel.Common.Enums.BookLibrary;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
#endregion

namespace Lumina.DataAccess.IntegrationTests.Core.UoW;

/// <summary>
/// Contains integration tests for the <see cref="LuminaDbContext"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LuminaDbContextTests : IDisposable
{
    private readonly string _connectionString;
    private readonly SqliteConnection _anchorConnection;
    private readonly LuminaDbContext _context;
    private readonly BookEntityFixture _bookEntityFixture = new();

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
        Assert.Equal(62, tableNames.Length);
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
        Assert.Contains("LibraryArtworkProviderConfigurations", tableNames);
        Assert.Contains("LibraryBookReaderConfigurations", tableNames);
        Assert.Contains("Tags", tableNames);
        Assert.Contains("Genres", tableNames);
        Assert.Contains("LibraryContentLocations", tableNames);
        Assert.Contains("LibraryPathTemplateParts", tableNames);
        Assert.Contains("BookTags", tableNames);
        Assert.Contains("BookGenres", tableNames);
        Assert.Contains("BookRatings", tableNames);
        Assert.Contains("BookISBNs", tableNames);
        Assert.Contains("Themes", tableNames);
        Assert.Contains("MediaContributors", tableNames);
        Assert.Contains("BookContributors", tableNames);
        Assert.Contains("BookArtwork", tableNames);
        Assert.Contains("ScheduledJobs", tableNames);
        Assert.Contains("ScheduledJobExecutions", tableNames);
        Assert.Contains("SchedulerDisplayPreferences", tableNames);
        Assert.Contains("Artists", tableNames);
        Assert.Contains("ArtistAliases", tableNames);
        Assert.Contains("ArtistIpis", tableNames);
        Assert.Contains("ArtistIsnis", tableNames);
        Assert.Contains("ArtistTags", tableNames);
        Assert.Contains("ArtistGenres", tableNames);
        Assert.Contains("ArtistRatings", tableNames);
        Assert.Contains("Albums", tableNames);
        Assert.Contains("AlbumReleaseTypes", tableNames);
        Assert.Contains("AlbumCatalogNumbers", tableNames);
        Assert.Contains("AlbumGenres", tableNames);
        Assert.Contains("AlbumRatings", tableNames);
        Assert.Contains("AlbumTags", tableNames);
        Assert.Contains("Tracks", tableNames);
        Assert.Contains("TrackContributors", tableNames);
        Assert.Contains("TrackGenres", tableNames);
        Assert.Contains("TrackIsrcs", tableNames);
        Assert.Contains("TrackMoods", tableNames);
        Assert.Contains("TrackRatings", tableNames);
        Assert.Contains("TrackTags", tableNames);
        Assert.Contains("TrackWorkLanguages", tableNames);
        Assert.Contains("TrackWorkIswcs", tableNames);
        Assert.Contains("ArtistContributors", tableNames);
        Assert.Contains("AlbumContributors", tableNames);
        Assert.Contains("MusicArtwork", tableNames);
        Assert.Contains("MusicLibraryScanItemMetadata", tableNames);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenAddingBook_ShouldPersistItToTheDatabase()
    {
        // Arrange
        _context.Database.EnsureCreated();
        BookEntity book = _bookEntityFixture.Create(path: "/books/test.epub", title: "Test Book", includeMetadata: false);
        book.MetadataStatus = MetadataStatus.Pending;
        book.CreatedOnUtc = DateTime.UtcNow;
        book.CreatedBy = Guid.NewGuid();
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
