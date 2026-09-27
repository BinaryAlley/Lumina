using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class MakesTrackPathUniquePerLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tracks_LibraryId_Path",
                table: "Tracks");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_LibraryId_Path",
                table: "Tracks",
                columns: new[] { "LibraryId", "Path" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tracks_LibraryId_Path",
                table: "Tracks");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_LibraryId_Path",
                table: "Tracks",
                columns: new[] { "LibraryId", "Path" });
        }
    }
}
