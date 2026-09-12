#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="BookContributorEntity"/> entity.
/// </summary>
public class BookContributorConfiguration : IEntityTypeConfiguration<BookContributorEntity>
{
    /// <summary>
    /// Configures the <see cref="BookContributorEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<BookContributorEntity> builder)
    {
        builder.ToTable("BookContributors");
        builder.HasKey(bookContributor => bookContributor.Id);
        builder.Property(bookContributor => bookContributor.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);

        builder.Property(bookContributor => bookContributor.BookId)
            .IsRequired()
            .HasColumnOrder(1);
        builder.Property(bookContributor => bookContributor.MediaContributorId)
            .IsRequired()
            .HasColumnOrder(2);
        builder.Property(bookContributor => bookContributor.Role)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired()
            .HasColumnOrder(3);

        // audit
        builder.Property(bookContributor => bookContributor.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(4);

        builder.Property(bookContributor => bookContributor.CreatedBy)
            .IsRequired()
            .HasColumnOrder(5);

        builder.Property(bookContributor => bookContributor.UpdatedOnUtc)
            .HasColumnOrder(6);

        builder.Property(bookContributor => bookContributor.UpdatedBy)
            .HasColumnOrder(7);

        builder.HasIndex(bookContributor => new { bookContributor.BookId, bookContributor.MediaContributorId, bookContributor.Role })
            .IsUnique();
        builder.HasIndex(bookContributor => bookContributor.MediaContributorId);
    }
}
