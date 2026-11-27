using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class MakesProviderConfigurationsUniquePerLibraryAndPlugin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The upserts read before they add, so two concurrent ones could store the same plugin twice for a library; the duplicates are removed before the
            // indexes become unique, keeping the configuration that is tried first, so the unique indexes can be created on an existing database.
            migrationBuilder.Sql("""
                DELETE FROM LibraryMetadataProviderConfigurations WHERE rowid NOT IN (
                    SELECT rowid FROM (
                        SELECT rowid, ROW_NUMBER() OVER (PARTITION BY LibraryId, PluginId ORDER BY Rank, rowid) AS RowNumber
                        FROM LibraryMetadataProviderConfigurations)
                    WHERE RowNumber = 1);

                DELETE FROM LibraryArtworkProviderConfigurations WHERE rowid NOT IN (
                    SELECT rowid FROM (
                        SELECT rowid, ROW_NUMBER() OVER (PARTITION BY LibraryId, PluginId ORDER BY Rank, rowid) AS RowNumber
                        FROM LibraryArtworkProviderConfigurations)
                    WHERE RowNumber = 1);
                """);

            migrationBuilder.DropIndex(
                name: "IX_LibraryMetadataProviderConfigurations_LibraryId_PluginId",
                table: "LibraryMetadataProviderConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_LibraryArtworkProviderConfigurations_LibraryId_PluginId",
                table: "LibraryArtworkProviderConfigurations");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryMetadataProviderConfigurations_LibraryId_PluginId",
                table: "LibraryMetadataProviderConfigurations",
                columns: new[] { "LibraryId", "PluginId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LibraryArtworkProviderConfigurations_LibraryId_PluginId",
                table: "LibraryArtworkProviderConfigurations",
                columns: new[] { "LibraryId", "PluginId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LibraryMetadataProviderConfigurations_LibraryId_PluginId",
                table: "LibraryMetadataProviderConfigurations");

            migrationBuilder.DropIndex(
                name: "IX_LibraryArtworkProviderConfigurations_LibraryId_PluginId",
                table: "LibraryArtworkProviderConfigurations");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryMetadataProviderConfigurations_LibraryId_PluginId",
                table: "LibraryMetadataProviderConfigurations",
                columns: new[] { "LibraryId", "PluginId" });

            migrationBuilder.CreateIndex(
                name: "IX_LibraryArtworkProviderConfigurations_LibraryId_PluginId",
                table: "LibraryArtworkProviderConfigurations",
                columns: new[] { "LibraryId", "PluginId" });
        }
    }
}
