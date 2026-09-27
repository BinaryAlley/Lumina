using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class MakesArtistNameUniquePerLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Artists_LibraryId_Name",
                table: "Artists");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_LibraryId_Name",
                table: "Artists",
                columns: new[] { "LibraryId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Artists_LibraryId_Name",
                table: "Artists");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_LibraryId_Name",
                table: "Artists",
                columns: new[] { "LibraryId", "Name" });
        }
    }
}
