using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddsUniqueBookLibraryIdPathIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Books_LibraryId_Path",
                table: "Books");

            migrationBuilder.CreateIndex(
                name: "IX_Books_LibraryId_Path",
                table: "Books",
                columns: new[] { "LibraryId", "Path" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Books_LibraryId_Path",
                table: "Books");

            migrationBuilder.CreateIndex(
                name: "IX_Books_LibraryId_Path",
                table: "Books",
                columns: new[] { "LibraryId", "Path" });
        }
    }
}
