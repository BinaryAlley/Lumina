using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddsAudioContainerToMusicMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Container",
                table: "Tracks",
                type: "TEXT",
                maxLength: 50,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 49);

            migrationBuilder.AddColumn<string>(
                name: "Container",
                table: "MusicLibraryScanItemMetadata",
                type: "TEXT",
                maxLength: 50,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 31);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Container",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "Container",
                table: "MusicLibraryScanItemMetadata");
        }
    }
}
