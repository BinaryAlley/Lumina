#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="TrackContributorEntity"/> entity.
/// </summary>
public class TrackContributorConfiguration : IEntityTypeConfiguration<TrackContributorEntity>
{
    /// <summary>
    /// Configures the <see cref="TrackContributorEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<TrackContributorEntity> builder)
    {
        builder.ToTable("TrackContributors");
        builder.HasKey(trackContributor => trackContributor.Id);
        builder.Property(trackContributor => trackContributor.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);

        builder.Property(trackContributor => trackContributor.TrackId)
            .IsRequired()
            .HasColumnOrder(1);
        builder.Property(trackContributor => trackContributor.MediaContributorId)
            .IsRequired()
            .HasColumnOrder(2);
        builder.Property(trackContributor => trackContributor.Role)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasColumnOrder(3);

        // audit
        builder.Property(trackContributor => trackContributor.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(4);

        builder.Property(trackContributor => trackContributor.CreatedBy)
            .IsRequired()
            .HasColumnOrder(5);

        builder.Property(trackContributor => trackContributor.UpdatedOnUtc)
            .HasColumnOrder(6);

        builder.Property(trackContributor => trackContributor.UpdatedBy)
            .HasColumnOrder(7);

        builder.HasIndex(trackContributor => new { trackContributor.TrackId, trackContributor.MediaContributorId, trackContributor.Role })
            .IsUnique();
        builder.HasIndex(trackContributor => trackContributor.MediaContributorId);
    }
}
