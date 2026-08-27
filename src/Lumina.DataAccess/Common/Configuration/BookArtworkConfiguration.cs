#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.WrittenContentLibrary.BookLibrary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#endregion

namespace Lumina.DataAccess.Common.Configuration;

/// <summary>
/// Configures the entity mapping for the <see cref="BookArtworkEntity"/> entity.
/// </summary>
public class BookArtworkConfiguration : IEntityTypeConfiguration<BookArtworkEntity>
{
    /// <summary>
    /// Configures the <see cref="BookArtworkEntity"/> entity.
    /// </summary>
    /// <param name="builder">The builder to be used to configure the entity.</param>
    public void Configure(EntityTypeBuilder<BookArtworkEntity> builder)
    {
        builder.ToTable("BookArtwork");
        builder.HasKey(bookArtwork => bookArtwork.Id);
        builder.Property(bookArtwork => bookArtwork.Id)
            .ValueGeneratedNever() // because EF always tries to generate the value for the Id, and because we generate it as part of the aggregate root, we need to tell EF not to generate it
            .HasColumnOrder(0);
        builder.Property(bookArtwork => bookArtwork.BookId)
            .IsRequired()
            .HasColumnOrder(1);
        builder.Property(bookArtwork => bookArtwork.ArtworkType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasColumnOrder(2);
        builder.Property(bookArtwork => bookArtwork.Ordinal)
            .IsRequired()
            .HasColumnOrder(3);
        builder.Property(bookArtwork => bookArtwork.FileName)
            .HasMaxLength(2048)
            .HasColumnOrder(4);
        builder.Property(bookArtwork => bookArtwork.ContentHash)
            .HasColumnOrder(5);
        builder.Property(bookArtwork => bookArtwork.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasColumnOrder(6);
        builder.Property(bookArtwork => bookArtwork.Provider)
            .HasMaxLength(100)
            .HasColumnOrder(7);
        builder.Property(bookArtwork => bookArtwork.LastUpdateUtc)
            .HasColumnOrder(8);

        // audit
        builder.Property(bookArtwork => bookArtwork.CreatedOnUtc)
            .IsRequired()
            .HasColumnOrder(9);

        builder.Property(bookArtwork => bookArtwork.CreatedBy)
            .IsRequired()
            .HasColumnOrder(10);

        builder.Property(bookArtwork => bookArtwork.UpdatedOnUtc)
            .HasDefaultValue(null)
            .HasColumnOrder(11);

        builder.Property(bookArtwork => bookArtwork.UpdatedBy)
            .HasDefaultValue(null)
            .HasColumnOrder(12);

        builder.HasIndex(bookArtwork => new { bookArtwork.BookId, bookArtwork.ArtworkType, bookArtwork.Ordinal })
            .IsUnique();
        builder.HasIndex(bookArtwork => bookArtwork.Status);
    }
}
