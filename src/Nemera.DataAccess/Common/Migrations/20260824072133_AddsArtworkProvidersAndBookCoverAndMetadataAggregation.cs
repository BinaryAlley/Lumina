using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddsArtworkProvidersAndBookCoverAndMetadataAggregation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IgnoreThePrefixForAlphaPicker",
                table: "UserSettings",
                newName: "ShouldIgnoreThePrefixForAlphaPicker");

            migrationBuilder.RenameColumn(
                name: "DownloadMetadataFromWeb",
                table: "Libraries",
                newName: "CanDownloadMetadataFromWeb");

            migrationBuilder.AddColumn<bool>(
                name: "ShouldAggregateMetadataWhenMissing",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<float>(
                name: "VolumeNumber",
                table: "Books",
                type: "REAL",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoverImagePath",
                table: "Books",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LibraryArtworkProviderConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LibraryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PluginId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    Rank = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryArtworkProviderConfigurations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LibraryArtworkProviderConfigurations_LibraryId_PluginId",
                table: "LibraryArtworkProviderConfigurations",
                columns: new[] { "LibraryId", "PluginId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LibraryArtworkProviderConfigurations");

            migrationBuilder.DropColumn(
                name: "ShouldAggregateMetadataWhenMissing",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "CoverImagePath",
                table: "Books");

            migrationBuilder.RenameColumn(
                name: "ShouldIgnoreThePrefixForAlphaPicker",
                table: "UserSettings",
                newName: "IgnoreThePrefixForAlphaPicker");

            migrationBuilder.RenameColumn(
                name: "CanDownloadMetadataFromWeb",
                table: "Libraries",
                newName: "DownloadMetadataFromWeb");

            migrationBuilder.AlterColumn<int>(
                name: "VolumeNumber",
                table: "Books",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(float),
                oldType: "REAL",
                oldNullable: true);
        }
    }
}
