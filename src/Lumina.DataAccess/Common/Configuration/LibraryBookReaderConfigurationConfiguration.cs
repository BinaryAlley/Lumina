#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="LibraryBookReaderConfigurationEntity"/> entity.
/// </summary>
public class LibraryBookReaderConfigurationConfiguration : IEntityTypeConfiguration<LibraryBookReaderConfigurationEntity>
{
    /// <summary>
    /// Configures the <see cref="LibraryBookReaderConfigurationEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<LibraryBookReaderConfigurationEntity> builder)
    {
        builder.ToTable("LibraryBookReaderConfigurations");
        builder.HasKey(configuration => configuration.Id);
        builder.Property(configuration => configuration.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);
        builder.Property(configuration => configuration.LibraryId)
            .IsRequired()
            .HasColumnOrder(1);
        builder.Property(configuration => configuration.PluginId)
            .IsRequired()
            .HasColumnOrder(2);
        builder.Property(configuration => configuration.IsEnabled)
            .IsRequired()
            .HasColumnOrder(3);

        // audit
        builder.Property(configuration => configuration.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(4);

        builder.Property(configuration => configuration.CreatedBy)
            .IsRequired()
            .HasColumnOrder(5);

        builder.Property(configuration => configuration.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(6);

        builder.Property(configuration => configuration.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(7);

        // A media library has at most one book reader configuration per plugin, so the pair is unique; the repository upserts on it, and
        // the constraint guarantees that two concurrent upserts cannot insert duplicate rows.
        builder.HasIndex(configuration => new { configuration.LibraryId, configuration.PluginId })
            .IsUnique();
    }
}
