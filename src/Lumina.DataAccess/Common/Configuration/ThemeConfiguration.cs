#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.Themes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="ThemeEntity"/> entity.
/// </summary>
public class ThemeConfiguration : IEntityTypeConfiguration<ThemeEntity>
{
    /// <summary>
    /// Configures the <see cref="ThemeEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<ThemeEntity> builder)
    {
        builder.ToTable("Themes");
        builder.HasKey(theme => theme.Id);
        builder.Property(theme => theme.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the install, we need to tell EF not to generate it
            .HasColumnOrder(0);

        // the manifest id is the business key used by the clients, so it must be unique
        builder.HasIndex(theme => theme.ThemeId)
            .IsUnique();

        builder.Property(theme => theme.ThemeId)
            .IsRequired()
            .HasMaxLength(64)
            .HasColumnOrder(1);

        builder.Property(theme => theme.Name)
            .IsRequired()
            .HasMaxLength(80)
            .HasColumnOrder(2);

        builder.Property(theme => theme.Description)
            .IsRequired()
            .HasMaxLength(300)
            .HasColumnOrder(3);

        builder.Property(theme => theme.Author)
            .IsRequired()
            .HasMaxLength(80)
            .HasColumnOrder(4);

        builder.Property(theme => theme.Version)
            .IsRequired()
            .HasMaxLength(40)
            .HasColumnOrder(5);

        builder.Property(theme => theme.PreviewPath)
            .HasMaxLength(240)
            .HasDefaultValue(null)
            .HasColumnOrder(6);

        builder.Property(theme => theme.InstallSource)
            .IsRequired()
            .HasConversion<string>()
            .HasColumnOrder(7);

        // at most one theme can be current: the filtered unique index guarantees it at the database level, since every row that is not current has a null value here, and nulls are allowed to repeat
        builder.HasIndex(theme => theme.IsCurrent)
            .IsUnique()
            .HasFilter("IsCurrent = 1");

        builder.Property(theme => theme.IsCurrent)
            .HasDefaultValue(null)
            .HasColumnOrder(8);

        builder.Property(theme => theme.IsDeleted)
            .HasDefaultValue(false)
            .HasColumnOrder(9);

        builder.Property(theme => theme.InstalledAtUtc)
            .IsRequired()
            .HasColumnOrder(10);

        // audit
        builder.Property(theme => theme.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(11);

        builder.Property(theme => theme.CreatedBy)
            .IsRequired()
            .HasColumnOrder(12);

        builder.Property(theme => theme.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(13);

        builder.Property(theme => theme.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(14);
    }
}
