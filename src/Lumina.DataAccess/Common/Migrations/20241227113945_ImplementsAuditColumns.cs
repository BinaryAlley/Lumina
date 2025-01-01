using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations;

/// <inheritdoc />
public partial class ImplementsAuditColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "Updated",
            table: "Libraries",
            newName: "UpdatedOnUtc");

        migrationBuilder.RenameColumn(
            name: "Created",
            table: "Libraries",
            newName: "CreatedOnUtc");

        migrationBuilder.RenameColumn(
            name: "Updated",
            table: "Books",
            newName: "UpdatedOnUtc");

        migrationBuilder.RenameColumn(
            name: "Created",
            table: "Books",
            newName: "CreatedOnUtc");

        migrationBuilder.AddColumn<Guid>(
            name: "CreatedBy",
            table: "Libraries",
            type: "TEXT",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AddColumn<Guid>(
            name: "UpdatedBy",
            table: "Libraries",
            type: "TEXT",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "CreatedBy",
            table: "Books",
            type: "TEXT",
            nullable: false,
            defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

        migrationBuilder.AddColumn<Guid>(
            name: "UpdatedBy",
            table: "Books",
            type: "TEXT",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "Libraries");

        migrationBuilder.DropColumn(
            name: "UpdatedBy",
            table: "Libraries");

        migrationBuilder.DropColumn(
            name: "CreatedBy",
            table: "Books");

        migrationBuilder.DropColumn(
            name: "UpdatedBy",
            table: "Books");

        migrationBuilder.RenameColumn(
            name: "UpdatedOnUtc",
            table: "Libraries",
            newName: "Updated");

        migrationBuilder.RenameColumn(
            name: "CreatedOnUtc",
            table: "Libraries",
            newName: "Created");

        migrationBuilder.RenameColumn(
            name: "UpdatedOnUtc",
            table: "Books",
            newName: "Updated");

        migrationBuilder.RenameColumn(
            name: "CreatedOnUtc",
            table: "Books",
            newName: "Created");
    }
}
