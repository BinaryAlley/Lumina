using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class RenamesLibraryScanSettingsColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SkipUnchangedDirectoriesDuringScan",
                table: "Libraries",
                newName: "ShouldSkipUnchangedDirectoriesDuringScan");

            migrationBuilder.RenameColumn(
                name: "SaveMetadataInMediaDirectories",
                table: "Libraries",
                newName: "ShouldSaveMetadataInMediaDirectories");

            migrationBuilder.RenameColumn(
                name: "DownloadMedatadaFromWeb",
                table: "Libraries",
                newName: "DownloadMetadataFromWeb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShouldSkipUnchangedDirectoriesDuringScan",
                table: "Libraries",
                newName: "SkipUnchangedDirectoriesDuringScan");

            migrationBuilder.RenameColumn(
                name: "ShouldSaveMetadataInMediaDirectories",
                table: "Libraries",
                newName: "SaveMetadataInMediaDirectories");

            migrationBuilder.RenameColumn(
                name: "DownloadMetadataFromWeb",
                table: "Libraries",
                newName: "DownloadMedatadaFromWeb");
        }
    }
}
