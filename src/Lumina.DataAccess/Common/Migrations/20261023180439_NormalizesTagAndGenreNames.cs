using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <summary>
    /// Normalizes the existing tag and genre names to their trimmed lowercase form, and merges the rows that differ only by casing.
    /// </summary>
    public partial class NormalizesTagAndGenreNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The value objects now store names trimmed and lowercased with the invariant culture, so the existing rows must be normalized before new
            // scans start reusing them by their normalized name. The SQL function is registered on every opened connection by
            // SqliteSharedReferenceNormalizationInterceptor, so non-ASCII uppercase letters are folded exactly like the value objects fold them.
            // The lowercase rows are inserted first, so that the join rows can always be repointed.
            migrationBuilder.Sql("""
                INSERT OR IGNORE INTO Tags (Name)
                SELECT DISTINCT lumina_normalize_shared_reference_name(Name) FROM Tags;

                INSERT OR IGNORE INTO Genres (Name)
                SELECT DISTINCT lumina_normalize_shared_reference_name(Name) FROM Genres;
                """);

            // A repointed join row that would collide with an existing lowercase join row is left in its old form by OR IGNORE, and is removed below.
            migrationBuilder.Sql("""
                UPDATE OR IGNORE TrackTags SET TagId = lumina_normalize_shared_reference_name(TagId) WHERE TagId <> lumina_normalize_shared_reference_name(TagId);
                UPDATE OR IGNORE AlbumTags SET TagId = lumina_normalize_shared_reference_name(TagId) WHERE TagId <> lumina_normalize_shared_reference_name(TagId);
                UPDATE OR IGNORE ArtistTags SET TagId = lumina_normalize_shared_reference_name(TagId) WHERE TagId <> lumina_normalize_shared_reference_name(TagId);
                UPDATE OR IGNORE BookTags SET TagId = lumina_normalize_shared_reference_name(TagId) WHERE TagId <> lumina_normalize_shared_reference_name(TagId);

                UPDATE OR IGNORE TrackGenres SET GenreId = lumina_normalize_shared_reference_name(GenreId) WHERE GenreId <> lumina_normalize_shared_reference_name(GenreId);
                UPDATE OR IGNORE AlbumGenres SET GenreId = lumina_normalize_shared_reference_name(GenreId) WHERE GenreId <> lumina_normalize_shared_reference_name(GenreId);
                UPDATE OR IGNORE ArtistGenres SET GenreId = lumina_normalize_shared_reference_name(GenreId) WHERE GenreId <> lumina_normalize_shared_reference_name(GenreId);
                UPDATE OR IGNORE BookGenres SET GenreId = lumina_normalize_shared_reference_name(GenreId) WHERE GenreId <> lumina_normalize_shared_reference_name(GenreId);
                """);

            // The rows still carrying a mixed-case or untrimmed name are the duplicates that could not be repointed, because the lowercase join row
            // was already present. They are removed before the old tag and genre rows, so that no foreign key is left pointing at a deleted row.
            migrationBuilder.Sql("""
                DELETE FROM TrackTags WHERE TagId <> lumina_normalize_shared_reference_name(TagId);
                DELETE FROM AlbumTags WHERE TagId <> lumina_normalize_shared_reference_name(TagId);
                DELETE FROM ArtistTags WHERE TagId <> lumina_normalize_shared_reference_name(TagId);
                DELETE FROM BookTags WHERE TagId <> lumina_normalize_shared_reference_name(TagId);

                DELETE FROM TrackGenres WHERE GenreId <> lumina_normalize_shared_reference_name(GenreId);
                DELETE FROM AlbumGenres WHERE GenreId <> lumina_normalize_shared_reference_name(GenreId);
                DELETE FROM ArtistGenres WHERE GenreId <> lumina_normalize_shared_reference_name(GenreId);
                DELETE FROM BookGenres WHERE GenreId <> lumina_normalize_shared_reference_name(GenreId);

                DELETE FROM Tags WHERE Name <> lumina_normalize_shared_reference_name(Name);
                DELETE FROM Genres WHERE Name <> lumina_normalize_shared_reference_name(Name);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Normalization is irreversible: the original letter casing of the names is not retained, so there is nothing to restore.
        }
    }
}
