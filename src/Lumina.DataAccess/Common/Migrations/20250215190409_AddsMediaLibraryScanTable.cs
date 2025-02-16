using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations;

/// <inheritdoc />
public partial class AddsMediaLibraryScanTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnUtc",
            table: "Libraries",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 10);

        migrationBuilder.AlterColumn<Guid>(
            name: "UpdatedBy",
            table: "Libraries",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 11);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnUtc",
            table: "Libraries",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "TEXT")
            .Annotation("Relational:ColumnOrder", 8);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            table: "Libraries",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "TEXT")
            .Annotation("Relational:ColumnOrder", 9);

        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnUtc",
            table: "Books",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 34)
            .OldAnnotation("Relational:ColumnOrder", 31);

        migrationBuilder.AlterColumn<Guid>(
            name: "UpdatedBy",
            table: "Books",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 35);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnUtc",
            table: "Books",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "TEXT")
            .Annotation("Relational:ColumnOrder", 32)
            .OldAnnotation("Relational:ColumnOrder", 30);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            table: "Books",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "TEXT")
            .Annotation("Relational:ColumnOrder", 33);

        migrationBuilder.CreateTable(
            name: "LibraryScans",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Status = table.Column<string>(type: "TEXT", nullable: false),
                CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                LibraryId = table.Column<Guid>(type: "TEXT", nullable: false),
                UserId = table.Column<Guid>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LibraryScans", x => x.Id);
                table.ForeignKey(
                    name: "FK_LibraryScans_Libraries_LibraryId",
                    column: x => x.LibraryId,
                    principalTable: "Libraries",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_LibraryScans_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_LibraryScans_LibraryId",
            table: "LibraryScans",
            column: "LibraryId");

        migrationBuilder.CreateIndex(
            name: "IX_LibraryScans_UserId",
            table: "LibraryScans",
            column: "UserId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "LibraryScans");

        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnUtc",
            table: "Libraries",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "TEXT",
            oldNullable: true)
            .OldAnnotation("Relational:ColumnOrder", 10);

        migrationBuilder.AlterColumn<Guid>(
            name: "UpdatedBy",
            table: "Libraries",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "TEXT",
            oldNullable: true)
            .OldAnnotation("Relational:ColumnOrder", 11);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnUtc",
            table: "Libraries",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "TEXT")
            .OldAnnotation("Relational:ColumnOrder", 8);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            table: "Libraries",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "TEXT")
            .OldAnnotation("Relational:ColumnOrder", 9);

        migrationBuilder.AlterColumn<DateTime>(
            name: "UpdatedOnUtc",
            table: "Books",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(DateTime),
            oldType: "TEXT",
            oldNullable: true)
            .Annotation("Relational:ColumnOrder", 31)
            .OldAnnotation("Relational:ColumnOrder", 34);

        migrationBuilder.AlterColumn<Guid>(
            name: "UpdatedBy",
            table: "Books",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(Guid),
            oldType: "TEXT",
            oldNullable: true)
            .OldAnnotation("Relational:ColumnOrder", 35);

        migrationBuilder.AlterColumn<DateTime>(
            name: "CreatedOnUtc",
            table: "Books",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(DateTime),
            oldType: "TEXT")
            .Annotation("Relational:ColumnOrder", 30)
            .OldAnnotation("Relational:ColumnOrder", 32);

        migrationBuilder.AlterColumn<Guid>(
            name: "CreatedBy",
            table: "Books",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "TEXT")
            .OldAnnotation("Relational:ColumnOrder", 33);
    }
}
