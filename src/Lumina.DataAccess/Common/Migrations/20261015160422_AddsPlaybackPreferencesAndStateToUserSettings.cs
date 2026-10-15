using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddsPlaybackPreferencesAndStateToUserSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsGaplessPlaybackEnabled",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: true)
                .Annotation("Relational:ColumnOrder", 16);

            migrationBuilder.AddColumn<bool>(
                name: "IsMuted",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 20);

            migrationBuilder.AddColumn<int>(
                name: "ReplayGainMode",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 17);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldAutoplay",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: true)
                .Annotation("Relational:ColumnOrder", 15);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldKeepDownloadingWhilePaused",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: true)
                .Annotation("Relational:ColumnOrder", 14);

            migrationBuilder.AddColumn<int>(
                name: "StreamingQuality",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Relational:ColumnOrder", 18);

            migrationBuilder.AddColumn<double>(
                name: "Volume",
                table: "UserSettings",
                type: "REAL",
                nullable: false,
                defaultValue: 1.0)
                .Annotation("Relational:ColumnOrder", 19);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsGaplessPlaybackEnabled",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "IsMuted",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ReplayGainMode",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ShouldAutoplay",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ShouldKeepDownloadingWhilePaused",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "StreamingQuality",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "Volume",
                table: "UserSettings");
        }
    }
}
