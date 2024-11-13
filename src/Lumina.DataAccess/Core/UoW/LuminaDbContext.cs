#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Lumina.Domain.Common.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public virtual DbSet<LibraryEntity> Libraries { get; set; } = null!;

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
        // find all the entity configurations in the assembly and apply them
        modelBuilder.Ignore<List<IDomainEvent>>()
                    .ApplyConfigurationsFromAssembly(typeof(LuminaDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    // TODO: implement async versions of SaveChanges

    /// <summary>
    /// Overrides the SaveChanges method of the DbContext class to automatically set the Created and Updated properties of entities that implement the <see cref="IStorageEntity"/>.
    /// </summary>
    /// <returns>The number of objects written to the underlying database.</returns>
    public override int SaveChanges()
    {
        // get all the entity entries that are either added or modified
        IEnumerable<EntityEntry> entries = ChangeTracker.Entries()
                                                        .Where(entity => entity.Entity is IStorageEntity &&
                                                                        (entity.State is EntityState.Added or EntityState.Modified));
        foreach (EntityEntry? entityEntry in entries)
        {
            // if the entity is in Added state, set the Created property to the current date and time
            if (entityEntry.State == EntityState.Added)
                ((IStorageEntity)entityEntry.Entity).Created = DateTime.UtcNow;
            // otherwise, if the entity is in Modified state, set the Updated property to the current date and time
            else if (entityEntry.State == EntityState.Modified)
                ((IStorageEntity)entityEntry.Entity).Updated = DateTime.UtcNow;
        }
        return base.SaveChanges();
    }
}
