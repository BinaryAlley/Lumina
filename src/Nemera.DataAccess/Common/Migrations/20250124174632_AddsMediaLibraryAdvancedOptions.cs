using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations;

/// <inheritdoc />
public partial class AddsMediaLibraryAdvancedOptions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "DownloadMedatadaFromWeb",
            table: "Libraries",
            type: "INTEGER",
            nullable: false,
            defaultValue: true)
            .Annotation("Relational:ColumnOrder", 6);

        migrationBuilder.AddColumn<bool>(
            name: "IsEnabled",
            table: "Libraries",
            type: "INTEGER",
            nullable: false,
            defaultValue: true)
            .Annotation("Relational:ColumnOrder", 4);

        migrationBuilder.AddColumn<bool>(
            name: "IsLocked",
            table: "Libraries",
            type: "INTEGER",
            nullable: false,
            defaultValue: false)
            .Annotation("Relational:ColumnOrder", 5);

        migrationBuilder.AddColumn<bool>(
            name: "SaveMetadataInMediaDirectories",
            table: "Libraries",
            type: "INTEGER",
            nullable: false,
            defaultValue: false)
            .Annotation("Relational:ColumnOrder", 7);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "DownloadMedatadaFromWeb",
            table: "Libraries");

        migrationBuilder.DropColumn(
            name: "IsEnabled",
            table: "Libraries");

        migrationBuilder.DropColumn(
            name: "IsLocked",
            table: "Libraries");

        migrationBuilder.DropColumn(
            name: "SaveMetadataInMediaDirectories",
            table: "Libraries");
    }
}
