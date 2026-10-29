#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Playlists;
using Lumina.Application.Common.DataAccess.Entities.UsersManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="PlaylistEntity"/> entity.
/// </summary>
public class PlaylistConfiguration : IEntityTypeConfiguration<PlaylistEntity>
{
    /// <summary>
    /// Configures the <see cref="PlaylistEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<PlaylistEntity> builder)
    {
        builder.ToTable("Playlists");
        builder.HasKey(playlist => playlist.Id);
        builder.Property(playlist => playlist.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);

        // A playlist belongs to the user that created it, and it is removed with that user.
        builder.HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(playlist => playlist.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(playlist => playlist.UserId)
            .IsRequired()
            .HasColumnOrder(1);

        builder.Property(playlist => playlist.Name)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnOrder(2);

        builder.Property(playlist => playlist.Description)
            .HasMaxLength(2000)
            .HasColumnOrder(3);

        builder.Property(playlist => playlist.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnOrder(4);

        // Audit.
        builder.Property(playlist => playlist.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(5);

        builder.Property(playlist => playlist.CreatedBy)
            .IsRequired()
            .HasColumnOrder(6);

        builder.Property(playlist => playlist.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(7);

        builder.Property(playlist => playlist.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(8);

        // The entry count is maintained by the set-based entry operations, so a window read never counts the entries of the whole playlist.
        builder.Property(playlist => playlist.EntryCount)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnOrder(10);

        // The concurrency token is incremented by every details edit and by every set-based entry operation, so a concurrent writer is
        // detected instead of silently overwriting the ordering work of another request.
        builder.Property(playlist => playlist.Version)
            .IsConcurrencyToken()
            .HasDefaultValue(0L)
            .HasColumnOrder(9);

        // The entries of the playlist are owned by the playlist: they are removed with it, and they are never referenced from elsewhere.
        builder.HasMany(playlist => playlist.Entries)
            .WithOne()
            .HasForeignKey(entry => entry.PlaylistId)
            .OnDelete(DeleteBehavior.Cascade);

        // Enables the list of the playlists of a user in the order of their most recent activity.
        builder.HasIndex(playlist => new { playlist.UserId, playlist.UpdatedOnUtc });
    }
}
