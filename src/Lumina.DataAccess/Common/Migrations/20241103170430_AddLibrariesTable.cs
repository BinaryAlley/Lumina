using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations;

/// <inheritdoc />
public partial class AddLibrariesTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Format",
            table: "Books",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT");

        migrationBuilder.AlterColumn<Guid>(
            name: "Id",
            table: "BookRatings",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER")
            .OldAnnotation("Sqlite:Autoincrement", true);

        migrationBuilder.AlterColumn<Guid>(
            name: "Id",
            table: "BookISBNs",
            type: "TEXT",
            nullable: false,
            oldClrType: typeof(int),
            oldType: "INTEGER")
            .OldAnnotation("Sqlite:Autoincrement", true);

        migrationBuilder.CreateTable(
            name: "Libraries",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                LibraryType = table.Column<string>(type: "TEXT", nullable: false),
                UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                Updated = table.Column<DateTime>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Libraries", x => x.Id);
                table.ForeignKey(
                    name: "FK_Libraries_Users_UserId",
                    column: x => x.UserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "LibraryContentLocations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Path = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                LibraryId = table.Column<Guid>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_LibraryContentLocations", x => x.Id);
                table.ForeignKey(
                    name: "FK_LibraryContentLocations_Libraries_LibraryId",
                    column: x => x.LibraryId,
                    principalTable: "Libraries",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Libraries_UserId",
            table: "Libraries",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_LibraryContentLocations_LibraryId",
            table: "LibraryContentLocations",
            column: "LibraryId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "LibraryContentLocations");

        migrationBuilder.DropTable(
            name: "Libraries");

        migrationBuilder.AlterColumn<string>(
            name: "Format",
            table: "Books",
            type: "TEXT",
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "Id",
            table: "BookRatings",
            type: "INTEGER",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "TEXT")
            .Annotation("Sqlite:Autoincrement", true);

        migrationBuilder.AlterColumn<int>(
            name: "Id",
            table: "BookISBNs",
            type: "INTEGER",
            nullable: false,
            oldClrType: typeof(Guid),
            oldType: "TEXT")
            .Annotation("Sqlite:Autoincrement", true);
    }
}
