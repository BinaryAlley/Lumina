using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddsLibraryBookReaderConfigurationsAndShouldRenderPdfAsImagesToUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShouldRenderPdfAsImages",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 11);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldPreserveBookStyles",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: true)
                .Annotation("Relational:ColumnOrder", 12);

            migrationBuilder.CreateTable(
                name: "LibraryBookReaderConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LibraryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PluginId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryBookReaderConfigurations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LibraryBookReaderConfigurations_LibraryId_PluginId",
                table: "LibraryBookReaderConfigurations",
                columns: new[] { "LibraryId", "PluginId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LibraryBookReaderConfigurations");

            migrationBuilder.DropColumn(
                name: "ShouldRenderPdfAsImages",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ShouldPreserveBookStyles",
                table: "UserSettings");
        }
    }
}
