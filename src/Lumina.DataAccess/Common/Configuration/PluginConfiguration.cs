#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Plugins;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="PluginEntity"/> entity.
/// </summary>
public class PluginConfiguration : IEntityTypeConfiguration<PluginEntity>
{
    /// <summary>
    /// Configures the <see cref="PluginEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<PluginEntity> builder)
    {
        builder.ToTable("Plugins");
        builder.HasKey(plugin => plugin.Id);
        builder.Property(plugin => plugin.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);
        builder.Property(plugin => plugin.Name)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnOrder(1);
        builder.Property(plugin => plugin.Author)
            .IsRequired()
            .HasMaxLength(255)
            .HasColumnOrder(2);
        builder.Property(plugin => plugin.Version)
            .IsRequired()
            .HasMaxLength(50)
            .HasColumnOrder(3);
        builder.Property(plugin => plugin.Description)
            .HasMaxLength(2000)
            .HasColumnOrder(4);
        builder.Property(plugin => plugin.LoadStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasColumnOrder(5);
        builder.Property(plugin => plugin.LoadError)
            .HasMaxLength(2000)
            .HasColumnOrder(6);
        builder.Property(plugin => plugin.SettingsJson)
            .HasColumnOrder(7);
        builder.Property(plugin => plugin.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(8);

        // audit
        builder.Property(plugin => plugin.CreatedBy)
            .IsRequired()
            .HasColumnOrder(9);

        builder.Property(plugin => plugin.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(10);

        builder.Property(plugin => plugin.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(11);
    }
}
