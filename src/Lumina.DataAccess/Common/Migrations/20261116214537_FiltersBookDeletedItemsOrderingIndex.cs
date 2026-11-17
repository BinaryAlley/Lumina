using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class FiltersBookDeletedItemsOrderingIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Books_LibraryId_IsDeleted_DeletedOnUtc_Title_Id",
                table: "Books");

            migrationBuilder.CreateIndex(
                name: "IX_Books_LibraryId_IsDeleted_DeletedOnUtc_Title_Id",
                table: "Books",
                columns: new[] { "LibraryId", "IsDeleted", "DeletedOnUtc", "Title", "Id" },
                filter: "\"IsDeleted\" = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Books_LibraryId_IsDeleted_DeletedOnUtc_Title_Id",
                table: "Books");

            migrationBuilder.CreateIndex(
                name: "IX_Books_LibraryId_IsDeleted_DeletedOnUtc_Title_Id",
                table: "Books",
                columns: new[] { "LibraryId", "IsDeleted", "DeletedOnUtc", "Title", "Id" });
        }
    }
}
