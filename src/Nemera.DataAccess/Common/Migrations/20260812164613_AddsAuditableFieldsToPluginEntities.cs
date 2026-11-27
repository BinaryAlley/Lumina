using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddsAuditableFieldsToPluginEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOnUtc",
                table: "Plugins",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 10)
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Plugins",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"))
                .Annotation("Relational:ColumnOrder", 9);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Plugins",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 11);

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "LibraryMetadataProviderConfigurations",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"))
                .Annotation("Relational:ColumnOrder", 6);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "LibraryMetadataProviderConfigurations",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "LibraryMetadataProviderConfigurations",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOnUtc",
                table: "LibraryMetadataProviderConfigurations",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 7);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Plugins");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Plugins");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "LibraryMetadataProviderConfigurations");

            migrationBuilder.DropColumn(
                name: "CreatedOnUtc",
                table: "LibraryMetadataProviderConfigurations");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "LibraryMetadataProviderConfigurations");

            migrationBuilder.DropColumn(
                name: "UpdatedOnUtc",
                table: "LibraryMetadataProviderConfigurations");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOnUtc",
                table: "Plugins",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 9)
                .OldAnnotation("Relational:ColumnOrder", 10);
        }
    }
}
