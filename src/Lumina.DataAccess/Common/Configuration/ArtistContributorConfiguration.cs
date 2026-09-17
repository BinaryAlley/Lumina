#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="ArtistContributorEntity"/> entity.
/// </summary>
public class ArtistContributorConfiguration : IEntityTypeConfiguration<ArtistContributorEntity>
{
    /// <summary>
    /// Configures the <see cref="ArtistContributorEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<ArtistContributorEntity> builder)
    {
        builder.ToTable("ArtistContributors");
        builder.HasKey(artistContributor => artistContributor.Id);
        builder.Property(artistContributor => artistContributor.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);

        builder.Property(artistContributor => artistContributor.ArtistId)
            .IsRequired()
            .HasColumnOrder(1);
        builder.Property(artistContributor => artistContributor.MediaContributorId)
            .IsRequired()
            .HasColumnOrder(2);
        builder.Property(artistContributor => artistContributor.Role)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasColumnOrder(3);

        // audit
        builder.Property(artistContributor => artistContributor.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(4);

        builder.Property(artistContributor => artistContributor.CreatedBy)
            .IsRequired()
            .HasColumnOrder(5);

        builder.Property(artistContributor => artistContributor.UpdatedOnUtc)
            .HasColumnOrder(6);

        builder.Property(artistContributor => artistContributor.UpdatedBy)
            .HasColumnOrder(7);

        builder.HasIndex(artistContributor => new { artistContributor.ArtistId, artistContributor.MediaContributorId, artistContributor.Role })
            .IsUnique();
        builder.HasIndex(artistContributor => artistContributor.MediaContributorId);
    }
}
