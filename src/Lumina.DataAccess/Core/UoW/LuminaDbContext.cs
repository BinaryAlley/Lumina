#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Authorization;
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Lumina.Application.Common.DataAccess.Entities.Scheduling;
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Microsoft.EntityFrameworkCore;
#endregion

namespace Lumina.DataAccess.Core.UoW;

/// <summary>
/// DbContext for the Lumina application.
/// </summary>
/// <remarks>To add a new migration: add-migration InitialMigration -p Lumina.DataAccess -s Lumina.Presentation.Api -o Common\Migrations</remarks>
/// <remarks>To update the database: update-database -p Lumina.DataAccess -s Lumina.Presentation.Api</remarks>
public class LuminaDbContext : DbContext
{
    public virtual DbSet<BookEntity> Books { get; set; } = null!;
    public virtual DbSet<UserEntity> Users { get; set; } = null!;
    public virtual DbSet<UserSettingsEntity> UserSettings { get; set; } = null!;
    public virtual DbSet<LibraryEntity> Libraries { get; set; } = null!;
    public virtual DbSet<LibraryScanEntity> LibraryScans { get; set; } = null!;
    public virtual DbSet<LibraryScanResultEntity> LibraryScanResults { get; set; } = null!;
    public virtual DbSet<LibraryScanSnapshotEntity> LibraryScanSnapshots { get; set; } = null!;
    public virtual DbSet<LibraryScanStagingResultsEntity> LibraryScanStagingResults { get; set; } = null!;
    public virtual DbSet<DirectoryScanFingerprintEntity> DirectoryScanFingerprints { get; set; } = null!;
    public virtual DbSet<RoleEntity> Roles { get; set; } = null!;
    public virtual DbSet<UserRoleEntity> UserRoles { get; set; } = null!;
    public virtual DbSet<PermissionEntity> Permissions { get; set; } = null!;
    public virtual DbSet<UserPermissionEntity> UserPermissions { get; set; } = null!;
    public virtual DbSet<RolePermissionEntity> RolePermissions { get; set; } = null!;
    public virtual DbSet<PluginEntity> Plugins { get; set; } = null!;
    public virtual DbSet<LibraryMetadataProviderConfigurationEntity> LibraryMetadataProviderConfigurations { get; set; } = null!;
    public virtual DbSet<LibraryArtworkProviderConfigurationEntity> LibraryArtworkProviderConfigurations { get; set; } = null!;
    public virtual DbSet<LibraryBookReaderConfigurationEntity> LibraryBookReaderConfigurations { get; set; } = null!;
    public virtual DbSet<ThemeEntity> Themes { get; set; } = null!;
    public virtual DbSet<MediaContributorEntity> MediaContributors { get; set; } = null!;
    public virtual DbSet<BookContributorEntity> BookContributors { get; set; } = null!;
    public virtual DbSet<ScheduledJobEntity> ScheduledJobs { get; set; } = null!;
    public virtual DbSet<ScheduledJobExecutionEntity> ScheduledJobExecutions { get; set; } = null!;

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

        base.OnModelCreating(modelBuilder);
    }
}
