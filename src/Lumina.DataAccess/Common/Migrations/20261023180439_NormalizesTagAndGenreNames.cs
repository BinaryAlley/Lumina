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
            // The value objects now store names trimmed and lowercased, so the existing rows must be normalized before new scans start reusing them
            // by their normalized name. SQLite's built-in lower() folds ASCII letters only, so a legacy name whose uppercase letters are outside
            // ASCII is not fully folded and is not merged with its lowercase counterpart. The lowercase rows are inserted first, so that the join
            // rows can always be repointed.
            migrationBuilder.Sql("""
                INSERT OR IGNORE INTO Tags (Name)
                SELECT DISTINCT trim(lower(Name)) FROM Tags;

                INSERT OR IGNORE INTO Genres (Name)
                SELECT DISTINCT trim(lower(Name)) FROM Genres;
                """);

            // A repointed join row that would collide with an existing lowercase join row is left in its old form by OR IGNORE, and is removed below.
            migrationBuilder.Sql("""
                UPDATE OR IGNORE TrackTags SET TagId = trim(lower(TagId)) WHERE TagId <> trim(lower(TagId));
                UPDATE OR IGNORE AlbumTags SET TagId = trim(lower(TagId)) WHERE TagId <> trim(lower(TagId));
                UPDATE OR IGNORE ArtistTags SET TagId = trim(lower(TagId)) WHERE TagId <> trim(lower(TagId));
                UPDATE OR IGNORE BookTags SET TagId = trim(lower(TagId)) WHERE TagId <> trim(lower(TagId));

                UPDATE OR IGNORE TrackGenres SET GenreId = trim(lower(GenreId)) WHERE GenreId <> trim(lower(GenreId));
                UPDATE OR IGNORE AlbumGenres SET GenreId = trim(lower(GenreId)) WHERE GenreId <> trim(lower(GenreId));
                UPDATE OR IGNORE ArtistGenres SET GenreId = trim(lower(GenreId)) WHERE GenreId <> trim(lower(GenreId));
                UPDATE OR IGNORE BookGenres SET GenreId = trim(lower(GenreId)) WHERE GenreId <> trim(lower(GenreId));
                """);

            // The rows still carrying a mixed-case or untrimmed name are the duplicates that could not be repointed, because the lowercase join row
            // was already present. They are removed before the old tag and genre rows, so that no foreign key is left pointing at a deleted row.
            migrationBuilder.Sql("""
                DELETE FROM TrackTags WHERE TagId <> trim(lower(TagId));
                DELETE FROM AlbumTags WHERE TagId <> trim(lower(TagId));
                DELETE FROM ArtistTags WHERE TagId <> trim(lower(TagId));
                DELETE FROM BookTags WHERE TagId <> trim(lower(TagId));

                DELETE FROM TrackGenres WHERE GenreId <> trim(lower(GenreId));
                DELETE FROM AlbumGenres WHERE GenreId <> trim(lower(GenreId));
                DELETE FROM ArtistGenres WHERE GenreId <> trim(lower(GenreId));
                DELETE FROM BookGenres WHERE GenreId <> trim(lower(GenreId));

                DELETE FROM Tags WHERE Name <> trim(lower(Name));
                DELETE FROM Genres WHERE Name <> trim(lower(Name));
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Normalization is irreversible: the original letter casing of the names is not retained, so there is nothing to restore.
        }
    }
}
