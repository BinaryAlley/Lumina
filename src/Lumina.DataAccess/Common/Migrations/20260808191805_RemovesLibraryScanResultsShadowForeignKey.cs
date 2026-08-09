using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class RemovesLibraryScanResultsShadowForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LibraryScanResults_LibraryScans_LibraryScanEntityId",
                table: "LibraryScanResults");

            migrationBuilder.DropIndex(
                name: "IX_LibraryScanResults_LibraryScanEntityId",
                table: "LibraryScanResults");

            migrationBuilder.DropColumn(
                name: "LibraryScanEntityId",
                table: "LibraryScanResults");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LibraryScanEntityId",
                table: "LibraryScanResults",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LibraryScanResults_LibraryScanEntityId",
                table: "LibraryScanResults",
                column: "LibraryScanEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_LibraryScanResults_LibraryScans_LibraryScanEntityId",
                table: "LibraryScanResults",
                column: "LibraryScanEntityId",
                principalTable: "LibraryScans",
                principalColumn: "Id");
        }
    }
}
