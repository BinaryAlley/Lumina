using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations;

/// <inheritdoc />
public partial class AddsLibraryScanResultTickColumn : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LastModified",
            table: "LibraryScanResults");

        migrationBuilder.AddColumn<long>(
            name: "Ticks",
            table: "LibraryScanResults",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0L)
            .Annotation("Relational:ColumnOrder", 4);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Ticks",
            table: "LibraryScanResults");

        migrationBuilder.AddColumn<DateTime>(
            name: "LastModified",
            table: "LibraryScanResults",
            type: "TEXT",
            nullable: false,
            defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified))
            .Annotation("Relational:ColumnOrder", 4);
    }
}
