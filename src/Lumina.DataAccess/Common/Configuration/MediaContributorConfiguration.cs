#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaContributors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="MediaContributorEntity"/> entity.
/// </summary>
public class MediaContributorConfiguration : IEntityTypeConfiguration<MediaContributorEntity>
{
    /// <summary>
    /// Configures the <see cref="MediaContributorEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<MediaContributorEntity> builder)
    {
        builder.ToTable("MediaContributors");
        builder.HasKey(contributor => contributor.Id);
        builder.Property(contributor => contributor.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);
        builder.Property(contributor => contributor.DisplayName)
            .IsRequired()
            .HasMaxLength(255)
            .UseCollation("NOCASE")
            .HasColumnOrder(1);
        builder.Property(contributor => contributor.LegalName)
            .HasMaxLength(255)
            .HasColumnOrder(2);
        builder.Property(contributor => contributor.Biography)
            .HasMaxLength(2000)
            .HasColumnOrder(3);
        builder.Property(contributor => contributor.DateOfBirth)
            .HasColumnOrder(4);
        builder.Property(contributor => contributor.DateOfDeath)
            .HasColumnOrder(5);

        // audit
        builder.Property(contributor => contributor.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(6);

        builder.Property(contributor => contributor.CreatedBy)
            .IsRequired()
            .HasColumnOrder(7);

        builder.Property(contributor => contributor.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(8);

        builder.Property(contributor => contributor.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(9);

        builder.HasIndex(contributor => contributor.DisplayName)
            .IsUnique();
    }
}
