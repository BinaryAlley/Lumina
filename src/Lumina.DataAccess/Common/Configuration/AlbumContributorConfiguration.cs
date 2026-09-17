#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="AlbumContributorEntity"/> entity.
/// </summary>
public class AlbumContributorConfiguration : IEntityTypeConfiguration<AlbumContributorEntity>
{
    /// <summary>
    /// Configures the <see cref="AlbumContributorEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<AlbumContributorEntity> builder)
    {
        builder.ToTable("AlbumContributors");
        builder.HasKey(albumContributor => albumContributor.Id);
        builder.Property(albumContributor => albumContributor.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);

        builder.Property(albumContributor => albumContributor.AlbumId)
            .IsRequired()
            .HasColumnOrder(1);
        builder.Property(albumContributor => albumContributor.MediaContributorId)
            .IsRequired()
            .HasColumnOrder(2);
        builder.Property(albumContributor => albumContributor.Role)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasColumnOrder(3);

        // audit
        builder.Property(albumContributor => albumContributor.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(4);

        builder.Property(albumContributor => albumContributor.CreatedBy)
            .IsRequired()
            .HasColumnOrder(5);

        builder.Property(albumContributor => albumContributor.UpdatedOnUtc)
            .HasColumnOrder(6);

        builder.Property(albumContributor => albumContributor.UpdatedBy)
            .HasColumnOrder(7);

        builder.HasIndex(albumContributor => new { albumContributor.AlbumId, albumContributor.MediaContributorId, albumContributor.Role })
            .IsUnique();
        builder.HasIndex(albumContributor => albumContributor.MediaContributorId);
    }
}
