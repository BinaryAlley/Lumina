using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddsMusicLibraryScanningAndMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Work",
                table: "Tracks",
                newName: "WorkTitle");

            migrationBuilder.RenameColumn(
                name: "ReleaseType",
                table: "Albums",
                newName: "Script");

            migrationBuilder.RenameColumn(
                name: "CatalogNumber",
                table: "Albums",
                newName: "Packaging");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOnUtc",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 39)
                .OldAnnotation("Relational:ColumnOrder", 36);

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdatedBy",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 40)
                .OldAnnotation("Relational:ColumnOrder", 37);

            migrationBuilder.AlterColumn<int>(
                name: "TrackNumber",
                table: "Tracks",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 26)
                .OldAnnotation("Relational:ColumnOrder", 25);

            migrationBuilder.AlterColumn<string>(
                name: "Script",
                table: "Tracks",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 28)
                .OldAnnotation("Relational:ColumnOrder", 27);

            migrationBuilder.AlterColumn<int>(
                name: "SampleRate",
                table: "Tracks",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 21)
                .OldAnnotation("Relational:ColumnOrder", 20);

            migrationBuilder.AlterColumn<string>(
                name: "ReleaseVersion",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 13)
                .OldAnnotation("Relational:ColumnOrder", 12);

            migrationBuilder.AlterColumn<string>(
                name: "ReleaseCountry",
                table: "Tracks",
                type: "TEXT",
                maxLength: 2,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 12)
                .OldAnnotation("Relational:ColumnOrder", 11);

            migrationBuilder.AlterColumn<int>(
                name: "ReReleaseYear",
                table: "Tracks",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 11)
                .OldAnnotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "ReReleaseDate",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 10)
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<int>(
                name: "OriginalReleaseYear",
                table: "Tracks",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 9)
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "OriginalReleaseDate",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalLanguageNativeName",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 19)
                .OldAnnotation("Relational:ColumnOrder", 18);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalLanguageName",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 18)
                .OldAnnotation("Relational:ColumnOrder", 17);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalLanguageCode",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 17)
                .OldAnnotation("Relational:ColumnOrder", 16);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicBrainzWorkId",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 36)
                .OldAnnotation("Relational:ColumnOrder", 33);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicBrainzTrackId",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 35)
                .OldAnnotation("Relational:ColumnOrder", 32);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicBrainzRecordingId",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 34)
                .OldAnnotation("Relational:ColumnOrder", 31);

            migrationBuilder.AlterColumn<string>(
                name: "LanguageNativeName",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 16)
                .OldAnnotation("Relational:ColumnOrder", 15);

            migrationBuilder.AlterColumn<string>(
                name: "LanguageName",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 15)
                .OldAnnotation("Relational:ColumnOrder", 14);

            migrationBuilder.AlterColumn<string>(
                name: "LanguageCode",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 14)
                .OldAnnotation("Relational:ColumnOrder", 13);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "Tracks",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 29)
                .OldAnnotation("Relational:ColumnOrder", 28);

            migrationBuilder.AlterColumn<int>(
                name: "DurationInSeconds",
                table: "Tracks",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 20)
                .OldAnnotation("Relational:ColumnOrder", 19);

            migrationBuilder.AlterColumn<int>(
                name: "DiscNumber",
                table: "Tracks",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 27)
                .OldAnnotation("Relational:ColumnOrder", 26);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "Tracks",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 37)
                .OldAnnotation("Relational:ColumnOrder", 34);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Tracks",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 38)
                .OldAnnotation("Relational:ColumnOrder", 35);

            migrationBuilder.AlterColumn<int>(
                name: "Channels",
                table: "Tracks",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 22)
                .OldAnnotation("Relational:ColumnOrder", 21);

            migrationBuilder.AlterColumn<int>(
                name: "Bpm",
                table: "Tracks",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 30)
                .OldAnnotation("Relational:ColumnOrder", 29);

            migrationBuilder.AlterColumn<int>(
                name: "Bitrate",
                table: "Tracks",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 25)
                .OldAnnotation("Relational:ColumnOrder", 24);

            migrationBuilder.AlterColumn<int>(
                name: "BitDepth",
                table: "Tracks",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 23)
                .OldAnnotation("Relational:ColumnOrder", 22);

            migrationBuilder.AlterColumn<string>(
                name: "AudioCodec",
                table: "Tracks",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 24)
                .OldAnnotation("Relational:ColumnOrder", 23);

            migrationBuilder.AlterColumn<string>(
                name: "WorkTitle",
                table: "Tracks",
                type: "TEXT",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 32)
                .OldAnnotation("Relational:ColumnOrder", 30);

            migrationBuilder.AddColumn<string>(
                name: "AcoustId",
                table: "Tracks",
                type: "TEXT",
                maxLength: 64,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 44);

            migrationBuilder.AddColumn<string>(
                name: "Disambiguation",
                table: "Tracks",
                type: "TEXT",
                maxLength: 255,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 7);

            migrationBuilder.AddColumn<bool>(
                name: "IsVideo",
                table: "Tracks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 31);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMetadataUpdateUtc",
                table: "Tracks",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 42);

            migrationBuilder.AddColumn<string>(
                name: "MetadataProvider",
                table: "Tracks",
                type: "TEXT",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 43);

            migrationBuilder.AddColumn<string>(
                name: "MetadataStatus",
                table: "Tracks",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending")
                .Annotation("Relational:ColumnOrder", 41);

            migrationBuilder.AddColumn<decimal>(
                name: "ReplayGainAlbumGain",
                table: "Tracks",
                type: "decimal(8,2)",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 47);

            migrationBuilder.AddColumn<decimal>(
                name: "ReplayGainAlbumPeak",
                table: "Tracks",
                type: "decimal(8,6)",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 48);

            migrationBuilder.AddColumn<decimal>(
                name: "ReplayGainTrackGain",
                table: "Tracks",
                type: "decimal(8,2)",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 45);

            migrationBuilder.AddColumn<decimal>(
                name: "ReplayGainTrackPeak",
                table: "Tracks",
                type: "decimal(8,6)",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 46);

            migrationBuilder.AddColumn<string>(
                name: "WorkType",
                table: "Tracks",
                type: "TEXT",
                maxLength: 50,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 33);

            migrationBuilder.AlterColumn<string>(
                name: "Website",
                table: "Artists",
                type: "TEXT",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2048,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 11)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOnUtc",
                table: "Artists",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 15)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdatedBy",
                table: "Artists",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 16)
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicBrainzArtistId",
                table: "Artists",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 12)
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "Artists",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 13)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Artists",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 14)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AddColumn<string>(
                name: "Area",
                table: "Artists",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BeginArea",
                table: "Artists",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Artists",
                type: "TEXT",
                maxLength: 2,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 7);

            migrationBuilder.AddColumn<string>(
                name: "Disambiguation",
                table: "Artists",
                type: "TEXT",
                maxLength: 255,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 4);

            migrationBuilder.AddColumn<string>(
                name: "EndArea",
                table: "Artists",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Artists",
                type: "TEXT",
                maxLength: 20,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 6);

            migrationBuilder.AddColumn<bool>(
                name: "IsEnded",
                table: "Artists",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 10);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMetadataUpdateUtc",
                table: "Artists",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 18);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LifeSpanBegin",
                table: "Artists",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 8);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LifeSpanEnd",
                table: "Artists",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 9);

            migrationBuilder.AddColumn<string>(
                name: "MetadataProvider",
                table: "Artists",
                type: "TEXT",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 19);

            migrationBuilder.AddColumn<string>(
                name: "MetadataStatus",
                table: "Artists",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending")
                .Annotation("Relational:ColumnOrder", 17);

            migrationBuilder.AddColumn<string>(
                name: "SortName",
                table: "Artists",
                type: "TEXT",
                maxLength: 255,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Artists",
                type: "TEXT",
                maxLength: 20,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOnUtc",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 34)
                .OldAnnotation("Relational:ColumnOrder", 30);

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdatedBy",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 35)
                .OldAnnotation("Relational:ColumnOrder", 31);

            migrationBuilder.AlterColumn<int>(
                name: "TotalTracks",
                table: "Albums",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 22)
                .OldAnnotation("Relational:ColumnOrder", 21);

            migrationBuilder.AlterColumn<int>(
                name: "TotalDiscs",
                table: "Albums",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 21)
                .OldAnnotation("Relational:ColumnOrder", 20);

            migrationBuilder.AlterColumn<string>(
                name: "ReleaseVersion",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 12)
                .OldAnnotation("Relational:ColumnOrder", 11);

            migrationBuilder.AlterColumn<string>(
                name: "ReleaseStatus",
                table: "Albums",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 20)
                .OldAnnotation("Relational:ColumnOrder", 19);

            migrationBuilder.AlterColumn<string>(
                name: "ReleaseCountry",
                table: "Albums",
                type: "TEXT",
                maxLength: 2,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 11)
                .OldAnnotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<int>(
                name: "ReReleaseYear",
                table: "Albums",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 10)
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "ReReleaseDate",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 9)
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<int>(
                name: "OriginalReleaseYear",
                table: "Albums",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "OriginalReleaseDate",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalLanguageNativeName",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 18)
                .OldAnnotation("Relational:ColumnOrder", 17);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalLanguageName",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 17)
                .OldAnnotation("Relational:ColumnOrder", 16);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalLanguageCode",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 16)
                .OldAnnotation("Relational:ColumnOrder", 15);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicBrainzReleaseId",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 29)
                .OldAnnotation("Relational:ColumnOrder", 25);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicBrainzReleaseGroupId",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 30)
                .OldAnnotation("Relational:ColumnOrder", 26);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicBrainzReleaseArtistId",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 31)
                .OldAnnotation("Relational:ColumnOrder", 27);

            migrationBuilder.AlterColumn<string>(
                name: "MediaFormat",
                table: "Albums",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 23)
                .OldAnnotation("Relational:ColumnOrder", 22);

            migrationBuilder.AlterColumn<string>(
                name: "LanguageNativeName",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 15)
                .OldAnnotation("Relational:ColumnOrder", 14);

            migrationBuilder.AlterColumn<string>(
                name: "LanguageName",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 14)
                .OldAnnotation("Relational:ColumnOrder", 13);

            migrationBuilder.AlterColumn<string>(
                name: "LanguageCode",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 13)
                .OldAnnotation("Relational:ColumnOrder", 12);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "Albums",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 32)
                .OldAnnotation("Relational:ColumnOrder", 28);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Albums",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 33)
                .OldAnnotation("Relational:ColumnOrder", 29);

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "Albums",
                type: "TEXT",
                maxLength: 13,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 13,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 25)
                .OldAnnotation("Relational:ColumnOrder", 23);

            migrationBuilder.AlterColumn<string>(
                name: "Script",
                table: "Albums",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 19)
                .OldAnnotation("Relational:ColumnOrder", 18);

            migrationBuilder.AddColumn<string>(
                name: "ASIN",
                table: "Albums",
                type: "TEXT",
                maxLength: 20,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 28);

            migrationBuilder.AddColumn<string>(
                name: "Disambiguation",
                table: "Albums",
                type: "TEXT",
                maxLength: 255,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 6);

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "Albums",
                type: "TEXT",
                maxLength: 255,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 27);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMetadataUpdateUtc",
                table: "Albums",
                type: "TEXT",
                nullable: true)
                .Annotation("Relational:ColumnOrder", 37);

            migrationBuilder.AddColumn<string>(
                name: "MetadataProvider",
                table: "Albums",
                type: "TEXT",
                maxLength: 100,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 38);

            migrationBuilder.AddColumn<string>(
                name: "MetadataStatus",
                table: "Albums",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pending")
                .Annotation("Relational:ColumnOrder", 36);

            migrationBuilder.AddColumn<string>(
                name: "ReleaseTitle",
                table: "Albums",
                type: "TEXT",
                maxLength: 255,
                nullable: true)
                .Annotation("Relational:ColumnOrder", 26);

            migrationBuilder.CreateTable(
                name: "AlbumCatalogNumbers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CatalogNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AlbumId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumCatalogNumbers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumCatalogNumbers_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumReleaseTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReleaseType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    AlbumId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumReleaseTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumReleaseTypes_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistAliases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    SortName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Type = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Locale = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    BeginDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    IsEnded = table.Column<bool>(type: "INTEGER", nullable: false),
                    ArtistId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistAliases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtistAliases_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistGenres",
                columns: table => new
                {
                    ArtistId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GenreId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistGenres", x => new { x.ArtistId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_ArtistGenres_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistGenres_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistIpis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ArtistId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistIpis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtistIpis_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistIsnis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ArtistId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistIsnis", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtistIsnis_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistRatings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ArtistId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    MaxValue = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    VoteCount = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtistRatings_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistTags",
                columns: table => new
                {
                    ArtistId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TagId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistTags", x => new { x.ArtistId, x.TagId });
                    table.ForeignKey(
                        name: "FK_ArtistTags_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MusicArtwork",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ArtworkType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Ordinal = table.Column<int>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    ContentHash = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Provider = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    LastUpdateUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicArtwork", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MusicLibraryScanItemMetadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LibraryScanId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LibraryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    ArtistName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ReleaseType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ReleaseYear = table.Column<int>(type: "INTEGER", nullable: true),
                    ReleaseName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    TrackTitle = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    TrackNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    DiscNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    DurationInSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    SampleRate = table.Column<int>(type: "INTEGER", nullable: false),
                    Channels = table.Column<int>(type: "INTEGER", nullable: false),
                    BitDepth = table.Column<int>(type: "INTEGER", nullable: true),
                    AudioCodec = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Bitrate = table.Column<int>(type: "INTEGER", nullable: true),
                    MusicBrainzArtistId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MusicBrainzReleaseArtistId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MusicBrainzReleaseGroupId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MusicBrainzReleaseId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MusicBrainzRecordingId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MusicBrainzTrackId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MusicBrainzWorkId = table.Column<Guid>(type: "TEXT", nullable: true),
                    WorkTitle = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    AcoustId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ReplayGainTrackGain = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    ReplayGainTrackPeak = table.Column<decimal>(type: "decimal(8,6)", nullable: true),
                    ReplayGainAlbumGain = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    ReplayGainAlbumPeak = table.Column<decimal>(type: "decimal(8,6)", nullable: true),
                    Moods = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicLibraryScanItemMetadata", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrackWorkIswcs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackWorkIswcs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackWorkIswcs_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackWorkLanguages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LanguageCode = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                    LanguageName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NativeName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackWorkLanguages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackWorkLanguages_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumCatalogNumbers_AlbumId",
                table: "AlbumCatalogNumbers",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumReleaseTypes_AlbumId",
                table: "AlbumReleaseTypes",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistAliases_ArtistId",
                table: "ArtistAliases",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistGenres_GenreId",
                table: "ArtistGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistIpis_ArtistId",
                table: "ArtistIpis",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistIsnis_ArtistId",
                table: "ArtistIsnis",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistRatings_ArtistId",
                table: "ArtistRatings",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistTags_TagId",
                table: "ArtistTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicArtwork_OwnerType_OwnerId_ArtworkType_Ordinal",
                table: "MusicArtwork",
                columns: new[] { "OwnerType", "OwnerId", "ArtworkType", "Ordinal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MusicArtwork_Status",
                table: "MusicArtwork",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MusicLibraryScanItemMetadata_LibraryScanId_Path",
                table: "MusicLibraryScanItemMetadata",
                columns: new[] { "LibraryScanId", "Path" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackWorkIswcs_TrackId",
                table: "TrackWorkIswcs",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackWorkLanguages_TrackId",
                table: "TrackWorkLanguages",
                column: "TrackId");

            migrationBuilder.AddColumn<bool>(
                name: "ShouldAggregateArtworkWhenMissing",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false)
                .Annotation("Relational:ColumnOrder", 13);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShouldAggregateArtworkWhenMissing",
                table: "UserSettings");

            migrationBuilder.DropTable(
                name: "AlbumCatalogNumbers");

            migrationBuilder.DropTable(
                name: "AlbumReleaseTypes");

            migrationBuilder.DropTable(
                name: "ArtistAliases");

            migrationBuilder.DropTable(
                name: "ArtistGenres");

            migrationBuilder.DropTable(
                name: "ArtistIpis");

            migrationBuilder.DropTable(
                name: "ArtistIsnis");

            migrationBuilder.DropTable(
                name: "ArtistRatings");

            migrationBuilder.DropTable(
                name: "ArtistTags");

            migrationBuilder.DropTable(
                name: "MusicArtwork");

            migrationBuilder.DropTable(
                name: "MusicLibraryScanItemMetadata");

            migrationBuilder.DropTable(
                name: "TrackWorkIswcs");

            migrationBuilder.DropTable(
                name: "TrackWorkLanguages");

            migrationBuilder.DropColumn(
                name: "AcoustId",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "Disambiguation",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "IsVideo",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "LastMetadataUpdateUtc",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "MetadataProvider",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "MetadataStatus",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "ReplayGainAlbumGain",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "ReplayGainAlbumPeak",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "ReplayGainTrackGain",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "ReplayGainTrackPeak",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "WorkType",
                table: "Tracks");

            migrationBuilder.DropColumn(
                name: "Area",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "BeginArea",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "Disambiguation",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "EndArea",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "IsEnded",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "LastMetadataUpdateUtc",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "LifeSpanBegin",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "LifeSpanEnd",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "MetadataProvider",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "MetadataStatus",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "SortName",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Artists");

            migrationBuilder.DropColumn(
                name: "ASIN",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "Disambiguation",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "LastMetadataUpdateUtc",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "MetadataProvider",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "MetadataStatus",
                table: "Albums");

            migrationBuilder.DropColumn(
                name: "ReleaseTitle",
                table: "Albums");

            migrationBuilder.RenameColumn(
                name: "WorkTitle",
                table: "Tracks",
                newName: "Work");

            migrationBuilder.RenameColumn(
                name: "Script",
                table: "Albums",
                newName: "ReleaseType");

            migrationBuilder.RenameColumn(
                name: "Packaging",
                table: "Albums",
                newName: "CatalogNumber");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOnUtc",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 36)
                .OldAnnotation("Relational:ColumnOrder", 39);

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdatedBy",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 37)
                .OldAnnotation("Relational:ColumnOrder", 40);

            migrationBuilder.AlterColumn<int>(
                name: "TrackNumber",
                table: "Tracks",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 25)
                .OldAnnotation("Relational:ColumnOrder", 26);

            migrationBuilder.AlterColumn<string>(
                name: "Script",
                table: "Tracks",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 27)
                .OldAnnotation("Relational:ColumnOrder", 28);

            migrationBuilder.AlterColumn<int>(
                name: "SampleRate",
                table: "Tracks",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 20)
                .OldAnnotation("Relational:ColumnOrder", 21);

            migrationBuilder.AlterColumn<string>(
                name: "ReleaseVersion",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 12)
                .OldAnnotation("Relational:ColumnOrder", 13);

            migrationBuilder.AlterColumn<string>(
                name: "ReleaseCountry",
                table: "Tracks",
                type: "TEXT",
                maxLength: 2,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 11)
                .OldAnnotation("Relational:ColumnOrder", 12);

            migrationBuilder.AlterColumn<int>(
                name: "ReReleaseYear",
                table: "Tracks",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 10)
                .OldAnnotation("Relational:ColumnOrder", 11);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "ReReleaseDate",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 9)
                .OldAnnotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<int>(
                name: "OriginalReleaseYear",
                table: "Tracks",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "OriginalReleaseDate",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalLanguageNativeName",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 18)
                .OldAnnotation("Relational:ColumnOrder", 19);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalLanguageName",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 17)
                .OldAnnotation("Relational:ColumnOrder", 18);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalLanguageCode",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 16)
                .OldAnnotation("Relational:ColumnOrder", 17);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicBrainzWorkId",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 33)
                .OldAnnotation("Relational:ColumnOrder", 36);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicBrainzTrackId",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 32)
                .OldAnnotation("Relational:ColumnOrder", 35);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicBrainzRecordingId",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 31)
                .OldAnnotation("Relational:ColumnOrder", 34);

            migrationBuilder.AlterColumn<string>(
                name: "LanguageNativeName",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 15)
                .OldAnnotation("Relational:ColumnOrder", 16);

            migrationBuilder.AlterColumn<string>(
                name: "LanguageName",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 14)
                .OldAnnotation("Relational:ColumnOrder", 15);

            migrationBuilder.AlterColumn<string>(
                name: "LanguageCode",
                table: "Tracks",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 13)
                .OldAnnotation("Relational:ColumnOrder", 14);

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "Tracks",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 28)
                .OldAnnotation("Relational:ColumnOrder", 29);

            migrationBuilder.AlterColumn<int>(
                name: "DurationInSeconds",
                table: "Tracks",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 19)
                .OldAnnotation("Relational:ColumnOrder", 20);

            migrationBuilder.AlterColumn<int>(
                name: "DiscNumber",
                table: "Tracks",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 26)
                .OldAnnotation("Relational:ColumnOrder", 27);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "Tracks",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 34)
                .OldAnnotation("Relational:ColumnOrder", 37);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Tracks",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 35)
                .OldAnnotation("Relational:ColumnOrder", 38);

            migrationBuilder.AlterColumn<int>(
                name: "Channels",
                table: "Tracks",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 21)
                .OldAnnotation("Relational:ColumnOrder", 22);

            migrationBuilder.AlterColumn<int>(
                name: "Bpm",
                table: "Tracks",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 29)
                .OldAnnotation("Relational:ColumnOrder", 30);

            migrationBuilder.AlterColumn<int>(
                name: "Bitrate",
                table: "Tracks",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 24)
                .OldAnnotation("Relational:ColumnOrder", 25);

            migrationBuilder.AlterColumn<int>(
                name: "BitDepth",
                table: "Tracks",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 22)
                .OldAnnotation("Relational:ColumnOrder", 23);

            migrationBuilder.AlterColumn<string>(
                name: "AudioCodec",
                table: "Tracks",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 23)
                .OldAnnotation("Relational:ColumnOrder", 24);

            migrationBuilder.AlterColumn<string>(
                name: "Work",
                table: "Tracks",
                type: "TEXT",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 255,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 30)
                .OldAnnotation("Relational:ColumnOrder", 32);

            migrationBuilder.AlterColumn<string>(
                name: "Website",
                table: "Artists",
                type: "TEXT",
                maxLength: 2048,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2048,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 11);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOnUtc",
                table: "Artists",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 15);

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdatedBy",
                table: "Artists",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("Relational:ColumnOrder", 16);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicBrainzArtistId",
                table: "Artists",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 4)
                .OldAnnotation("Relational:ColumnOrder", 12);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "Artists",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 13);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Artists",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("Relational:ColumnOrder", 14);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOnUtc",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 30)
                .OldAnnotation("Relational:ColumnOrder", 34);

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdatedBy",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 31)
                .OldAnnotation("Relational:ColumnOrder", 35);

            migrationBuilder.AlterColumn<int>(
                name: "TotalTracks",
                table: "Albums",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Relational:ColumnOrder", 21)
                .OldAnnotation("Relational:ColumnOrder", 22);

            migrationBuilder.AlterColumn<int>(
                name: "TotalDiscs",
                table: "Albums",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 20)
                .OldAnnotation("Relational:ColumnOrder", 21);

            migrationBuilder.AlterColumn<string>(
                name: "ReleaseVersion",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 11)
                .OldAnnotation("Relational:ColumnOrder", 12);

            migrationBuilder.AlterColumn<string>(
                name: "ReleaseStatus",
                table: "Albums",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 19)
                .OldAnnotation("Relational:ColumnOrder", 20);

            migrationBuilder.AlterColumn<string>(
                name: "ReleaseCountry",
                table: "Albums",
                type: "TEXT",
                maxLength: 2,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 10)
                .OldAnnotation("Relational:ColumnOrder", 11);

            migrationBuilder.AlterColumn<int>(
                name: "ReReleaseYear",
                table: "Albums",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 9)
                .OldAnnotation("Relational:ColumnOrder", 10);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "ReReleaseDate",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("Relational:ColumnOrder", 9);

            migrationBuilder.AlterColumn<int>(
                name: "OriginalReleaseYear",
                table: "Albums",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<DateOnly>(
                name: "OriginalReleaseDate",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateOnly),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalLanguageNativeName",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 17)
                .OldAnnotation("Relational:ColumnOrder", 18);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalLanguageName",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 16)
                .OldAnnotation("Relational:ColumnOrder", 17);

            migrationBuilder.AlterColumn<string>(
                name: "OriginalLanguageCode",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 15)
                .OldAnnotation("Relational:ColumnOrder", 16);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicBrainzReleaseId",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 25)
                .OldAnnotation("Relational:ColumnOrder", 29);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicBrainzReleaseGroupId",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 26)
                .OldAnnotation("Relational:ColumnOrder", 30);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicBrainzReleaseArtistId",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 27)
                .OldAnnotation("Relational:ColumnOrder", 31);

            migrationBuilder.AlterColumn<string>(
                name: "MediaFormat",
                table: "Albums",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 22)
                .OldAnnotation("Relational:ColumnOrder", 23);

            migrationBuilder.AlterColumn<string>(
                name: "LanguageNativeName",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 14)
                .OldAnnotation("Relational:ColumnOrder", 15);

            migrationBuilder.AlterColumn<string>(
                name: "LanguageName",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 13)
                .OldAnnotation("Relational:ColumnOrder", 14);

            migrationBuilder.AlterColumn<string>(
                name: "LanguageCode",
                table: "Albums",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 12)
                .OldAnnotation("Relational:ColumnOrder", 13);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "Albums",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 28)
                .OldAnnotation("Relational:ColumnOrder", 32);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "Albums",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 29)
                .OldAnnotation("Relational:ColumnOrder", 33);

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "Albums",
                type: "TEXT",
                maxLength: 13,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 13,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 23)
                .OldAnnotation("Relational:ColumnOrder", 25);

            migrationBuilder.AlterColumn<string>(
                name: "ReleaseType",
                table: "Albums",
                type: "TEXT",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 18)
                .OldAnnotation("Relational:ColumnOrder", 19);
        }
    }
}
