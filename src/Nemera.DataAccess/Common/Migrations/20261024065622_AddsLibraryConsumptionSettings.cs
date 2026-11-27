using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddsLibraryConsumptionSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompletionThresholdPercentage",
                table: "Libraries",
                type: "INTEGER",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 17);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldTrackConsumptionProgress",
                table: "Libraries",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 16);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionThresholdPercentage",
                table: "Libraries");

            migrationBuilder.DropColumn(
                name: "ShouldTrackConsumptionProgress",
                table: "Libraries");
        }
    }
}
