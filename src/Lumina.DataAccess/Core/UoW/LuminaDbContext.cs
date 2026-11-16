#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.Favorites;
using Lumina.Application.Common.DataAccess.Entities.MediaConsumption;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.DataAccess.Common.ValueGeneration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
#endregion

namespace Lumina.DataAccess.Core.UoW;

/// <summary>
/// DbContext for the Lumina application.
/// </summary>
/// <remarks>To add a new migration: add-migration InitialMigration -p Lumina.DataAccess -s Lumina.Presentation.Api -o Common\Migrations</remarks>
/// <remarks>To update the database: update-database -p Lumina.DataAccess -s Lumina.Presentation.Api</remarks>
public class LuminaDbContext : DbContext
{
    /// <summary>
    /// Gets or sets the books stored in the database.
    /// </summary>
    public virtual DbSet<BookEntity> Books { get; set; } = null!;

    /// <summary>
    /// Gets or sets the artists stored in the database.
    /// </summary>
    public virtual DbSet<ArtistEntity> Artists { get; set; } = null!;

    /// <summary>
    /// Gets or sets the albums stored in the database.
    /// </summary>
    public virtual DbSet<AlbumEntity> Albums { get; set; } = null!;

    /// <summary>
    /// Gets or sets the tracks stored in the database.
    /// </summary>
    public virtual DbSet<TrackEntity> Tracks { get; set; } = null!;

    /// <summary>
    /// Gets or sets the artwork of the music library items stored in the database.
    /// </summary>
    public virtual DbSet<MusicArtworkEntity> MusicArtwork { get; set; } = null!;

    /// <summary>
    /// Gets or sets the metadata extracted from the music files discovered during a scan, stored in the database.
    /// </summary>
    public virtual DbSet<MusicLibraryScanItemMetadataEntity> MusicLibraryScanItemMetadata { get; set; } = null!;

    /// <summary>
    /// Gets or sets the users stored in the database.
    /// </summary>
    public virtual DbSet<UserEntity> Users { get; set; } = null!;

    /// <summary>
    /// Gets or sets the settings of the users stored in the database.
    /// </summary>
    public virtual DbSet<UserSettingsEntity> UserSettings { get; set; } = null!;

    /// <summary>
    /// Gets or sets the media libraries stored in the database.
    /// </summary>
    public virtual DbSet<LibraryEntity> Libraries { get; set; } = null!;

    /// <summary>
    /// Gets or sets the scans of the media libraries stored in the database.
    /// </summary>
    public virtual DbSet<LibraryScanEntity> LibraryScans { get; set; } = null!;

    /// <summary>
    /// Gets or sets the results of the media library scans stored in the database.
    /// </summary>
    public virtual DbSet<LibraryScanResultEntity> LibraryScanResults { get; set; } = null!;

    /// <summary>
    /// Gets or sets the snapshots of the media library items produced by the scans, stored in the database.
    /// </summary>
    public virtual DbSet<LibraryScanSnapshotEntity> LibraryScanSnapshots { get; set; } = null!;

    /// <summary>
    /// Gets or sets the staging results of the media library scans, holding the discovered file system items, stored in the database.
    /// </summary>
    public virtual DbSet<LibraryScanStagingResultsEntity> LibraryScanStagingResults { get; set; } = null!;

    /// <summary>
    /// Gets or sets the fingerprints of the scanned directories, holding their last write time, stored in the database.
    /// </summary>
    public virtual DbSet<DirectoryScanFingerprintEntity> DirectoryScanFingerprints { get; set; } = null!;

    /// <summary>
    /// Gets or sets the authorization roles stored in the database.
    /// </summary>
    public virtual DbSet<RoleEntity> Roles { get; set; } = null!;

    /// <summary>
    /// Gets or sets the associations between users and roles stored in the database.
    /// </summary>
    public virtual DbSet<UserRoleEntity> UserRoles { get; set; } = null!;

    /// <summary>
    /// Gets or sets the authorization permissions stored in the database.
    /// </summary>
    public virtual DbSet<PermissionEntity> Permissions { get; set; } = null!;

    /// <summary>
    /// Gets or sets the associations between users and permissions stored in the database.
    /// </summary>
    public virtual DbSet<UserPermissionEntity> UserPermissions { get; set; } = null!;

    /// <summary>
    /// Gets or sets the associations between roles and permissions stored in the database.
    /// </summary>
    public virtual DbSet<RolePermissionEntity> RolePermissions { get; set; } = null!;

    /// <summary>
    /// Gets or sets the plugins stored in the database.
    /// </summary>
    public virtual DbSet<PluginEntity> Plugins { get; set; } = null!;

    /// <summary>
    /// Gets or sets the metadata provider configurations of the media libraries stored in the database.
    /// </summary>
    public virtual DbSet<LibraryMetadataProviderConfigurationEntity> LibraryMetadataProviderConfigurations { get; set; } = null!;

    /// <summary>
    /// Gets or sets the artwork provider configurations of the media libraries stored in the database.
    /// </summary>
    public virtual DbSet<LibraryArtworkProviderConfigurationEntity> LibraryArtworkProviderConfigurations { get; set; } = null!;

    /// <summary>
    /// Gets or sets the book reader configurations of the media libraries stored in the database.
    /// </summary>
    public virtual DbSet<LibraryBookReaderConfigurationEntity> LibraryBookReaderConfigurations { get; set; } = null!;

    /// <summary>
    /// Gets or sets the themes stored in the database.
    /// </summary>
    public virtual DbSet<ThemeEntity> Themes { get; set; } = null!;

    /// <summary>
    /// Gets or sets the media contributors stored in the database.
    /// </summary>
    public virtual DbSet<MediaContributorEntity> MediaContributors { get; set; } = null!;

    /// <summary>
    /// Gets or sets the participations of the media contributors in books stored in the database.
    /// </summary>
    public virtual DbSet<BookContributorEntity> BookContributors { get; set; } = null!;

    /// <summary>
    /// Gets or sets the scheduled jobs stored in the database.
    /// </summary>
    public virtual DbSet<ScheduledJobEntity> ScheduledJobs { get; set; } = null!;

    /// <summary>
    /// Gets or sets the executions of the scheduled jobs stored in the database.
    /// </summary>
    public virtual DbSet<ScheduledJobExecutionEntity> ScheduledJobExecutions { get; set; } = null!;

    /// <summary>
    /// Gets or sets the display preferences of the scheduler page of the users stored in the database.
    /// </summary>
    public virtual DbSet<SchedulerDisplayPreferencesEntity> SchedulerDisplayPreferences { get; set; } = null!;

    /// <summary>
    /// Gets or sets the playlists stored in the database.
    /// </summary>
    public virtual DbSet<PlaylistEntity> Playlists { get; set; } = null!;

    /// <summary>
    /// Gets or sets the entries of the playlists stored in the database.
    /// </summary>
    public virtual DbSet<PlaylistEntryEntity> PlaylistEntries { get; set; } = null!;

    /// <summary>
    /// Gets or sets the favorites stored in the database.
    /// </summary>
    public virtual DbSet<FavoriteEntity> Favorites { get; set; } = null!;

    /// <summary>
    /// Gets or sets the media consumption records stored in the database.
    /// </summary>
    public virtual DbSet<MediaConsumptionRecordEntity> MediaConsumptionRecords { get; set; } = null!;

    /// <summary>
    /// Gets or sets the media resume points stored in the database.
    /// </summary>
    public virtual DbSet<MediaResumePointEntity> MediaResumePoints { get; set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="LuminaDbContext"/> class.
    /// </summary>
    /// <param name="options">The options for configuring the database context.</param>
    public LuminaDbContext(DbContextOptions<LuminaDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Configures the entity mappings for the database context.
    /// </summary>
    /// <param name="modelBuilder">The model builder used to configure the entity mappings.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Find all the entity configurations in the assembly and apply them.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LuminaDbContext).Assembly);

        // The relational provider seeds its Guid value generators from Guid.NewGuid, so replace them with a time ordered generator for every
        // identifier that the persistence layer assigns itself. This covers real keys, such as PermissionEntity.Id, and the shadow keys of
        // owned collections, such as AlbumReleaseTypes.Id.
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
            foreach (IMutableProperty property in entityType.GetProperties())
                if (property.ClrType == typeof(Guid) && property.ValueGenerated == ValueGenerated.OnAdd)
                    property.SetValueGeneratorFactory((generatedProperty, declaringType) => new GuidV7ValueGenerator());

        base.OnModelCreating(modelBuilder);
    }
}
