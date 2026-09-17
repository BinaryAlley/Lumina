using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumina.DataAccess.Common.Migrations
{
    /// <inheritdoc />
    public partial class AddsMusicLibraryAndCanonicalContributorRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BookContributors_BookId_MediaContributorId",
                table: "BookContributors");

            migrationBuilder.DropColumn(
                name: "RoleName",
                table: "BookContributors");

            migrationBuilder.RenameColumn(
                name: "RoleCategory",
                table: "BookContributors",
                newName: "Role");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOnUtc",
                table: "BookContributors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdatedBy",
                table: "BookContributors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 8);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "BookContributors",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 4)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "BookContributors",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "BookContributors",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50)
                .Annotation("Relational:ColumnOrder", 3)
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.CreateTable(
                name: "Artists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    LibraryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Website = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: true),
                    MusicBrainzArtistId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ArtistId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LibraryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    OriginalTitle = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    OriginalReleaseDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    OriginalReleaseYear = table.Column<int>(type: "INTEGER", nullable: true),
                    ReReleaseDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    ReReleaseYear = table.Column<int>(type: "INTEGER", nullable: true),
                    ReleaseCountry = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    ReleaseVersion = table.Column<string>(type: "TEXT", nullable: true),
                    LanguageCode = table.Column<string>(type: "TEXT", nullable: true),
                    LanguageName = table.Column<string>(type: "TEXT", nullable: true),
                    LanguageNativeName = table.Column<string>(type: "TEXT", nullable: true),
                    OriginalLanguageCode = table.Column<string>(type: "TEXT", nullable: true),
                    OriginalLanguageName = table.Column<string>(type: "TEXT", nullable: true),
                    OriginalLanguageNativeName = table.Column<string>(type: "TEXT", nullable: true),
                    ReleaseType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ReleaseStatus = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    TotalDiscs = table.Column<int>(type: "INTEGER", nullable: true),
                    TotalTracks = table.Column<int>(type: "INTEGER", nullable: false),
                    MediaFormat = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Barcode = table.Column<string>(type: "TEXT", maxLength: 13, nullable: true),
                    CatalogNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    MusicBrainzReleaseId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MusicBrainzReleaseGroupId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MusicBrainzReleaseArtistId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Albums_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistContributors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ArtistId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MediaContributorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistContributors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtistContributors_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumContributors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlbumId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MediaContributorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumContributors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumContributors_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumGenres",
                columns: table => new
                {
                    AlbumId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GenreId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumGenres", x => new { x.AlbumId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_AlbumGenres_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumGenres_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumRatings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    AlbumId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    MaxValue = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    VoteCount = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlbumRatings_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AlbumTags",
                columns: table => new
                {
                    AlbumId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TagId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlbumTags", x => new { x.AlbumId, x.TagId });
                    table.ForeignKey(
                        name: "FK_AlbumTags_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AlbumTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tracks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlbumId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LibraryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Path = table.Column<string>(type: "TEXT", maxLength: 2048, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    OriginalTitle = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    OriginalReleaseDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    OriginalReleaseYear = table.Column<int>(type: "INTEGER", nullable: true),
                    ReReleaseDate = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    ReReleaseYear = table.Column<int>(type: "INTEGER", nullable: true),
                    ReleaseCountry = table.Column<string>(type: "TEXT", maxLength: 2, nullable: true),
                    ReleaseVersion = table.Column<string>(type: "TEXT", nullable: true),
                    LanguageCode = table.Column<string>(type: "TEXT", nullable: true),
                    LanguageName = table.Column<string>(type: "TEXT", nullable: true),
                    LanguageNativeName = table.Column<string>(type: "TEXT", nullable: true),
                    OriginalLanguageCode = table.Column<string>(type: "TEXT", nullable: true),
                    OriginalLanguageName = table.Column<string>(type: "TEXT", nullable: true),
                    OriginalLanguageNativeName = table.Column<string>(type: "TEXT", nullable: true),
                    DurationInSeconds = table.Column<int>(type: "INTEGER", nullable: false),
                    SampleRate = table.Column<int>(type: "INTEGER", nullable: false),
                    Channels = table.Column<int>(type: "INTEGER", nullable: false),
                    BitDepth = table.Column<int>(type: "INTEGER", nullable: true),
                    AudioCodec = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Bitrate = table.Column<int>(type: "INTEGER", nullable: true),
                    TrackNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    DiscNumber = table.Column<int>(type: "INTEGER", nullable: true),
                    Script = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Key = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Bpm = table.Column<int>(type: "INTEGER", nullable: true),
                    Work = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    MusicBrainzRecordingId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MusicBrainzTrackId = table.Column<Guid>(type: "TEXT", nullable: true),
                    MusicBrainzWorkId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tracks_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackContributors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MediaContributorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackContributors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackContributors_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackGenres",
                columns: table => new
                {
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GenreId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackGenres", x => new { x.TrackId, x.GenreId });
                    table.ForeignKey(
                        name: "FK_TrackGenres_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackGenres_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackIsrcs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ISRC = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackIsrcs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackIsrcs_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackMoods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackMoods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackMoods_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackRatings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    MaxValue = table.Column<decimal>(type: "decimal(3,2)", nullable: false),
                    VoteCount = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackRatings_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrackTags",
                columns: table => new
                {
                    TrackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TagId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackTags", x => new { x.TrackId, x.TagId });
                    table.ForeignKey(
                        name: "FK_TrackTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackTags_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookContributors_BookId_MediaContributorId_Role",
                table: "BookContributors",
                columns: new[] { "BookId", "MediaContributorId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlbumContributors_AlbumId_MediaContributorId_Role",
                table: "AlbumContributors",
                columns: new[] { "AlbumId", "MediaContributorId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlbumContributors_MediaContributorId",
                table: "AlbumContributors",
                column: "MediaContributorId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumGenres_GenreId",
                table: "AlbumGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_AlbumRatings_AlbumId",
                table: "AlbumRatings",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_ArtistId_Title",
                table: "Albums",
                columns: new[] { "ArtistId", "Title" });

            migrationBuilder.CreateIndex(
                name: "IX_Albums_LibraryId_Title",
                table: "Albums",
                columns: new[] { "LibraryId", "Title" });

            migrationBuilder.CreateIndex(
                name: "IX_AlbumTags_TagId",
                table: "AlbumTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistContributors_ArtistId_MediaContributorId_Role",
                table: "ArtistContributors",
                columns: new[] { "ArtistId", "MediaContributorId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArtistContributors_MediaContributorId",
                table: "ArtistContributors",
                column: "MediaContributorId");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_LibraryId_Name",
                table: "Artists",
                columns: new[] { "LibraryId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_TrackContributors_MediaContributorId",
                table: "TrackContributors",
                column: "MediaContributorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackContributors_TrackId_MediaContributorId_Role",
                table: "TrackContributors",
                columns: new[] { "TrackId", "MediaContributorId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackGenres_GenreId",
                table: "TrackGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackIsrcs_TrackId",
                table: "TrackIsrcs",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackMoods_TrackId",
                table: "TrackMoods",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackRatings_TrackId",
                table: "TrackRatings",
                column: "TrackId");

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_AlbumId_TrackNumber",
                table: "Tracks",
                columns: new[] { "AlbumId", "TrackNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_Tracks_LibraryId_Path",
                table: "Tracks",
                columns: new[] { "LibraryId", "Path" });

            migrationBuilder.CreateIndex(
                name: "IX_TrackTags_TagId",
                table: "TrackTags",
                column: "TagId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlbumContributors");

            migrationBuilder.DropTable(
                name: "AlbumGenres");

            migrationBuilder.DropTable(
                name: "AlbumRatings");

            migrationBuilder.DropTable(
                name: "AlbumTags");

            migrationBuilder.DropTable(
                name: "ArtistContributors");

            migrationBuilder.DropTable(
                name: "TrackContributors");

            migrationBuilder.DropTable(
                name: "TrackGenres");

            migrationBuilder.DropTable(
                name: "TrackIsrcs");

            migrationBuilder.DropTable(
                name: "TrackMoods");

            migrationBuilder.DropTable(
                name: "TrackRatings");

            migrationBuilder.DropTable(
                name: "TrackTags");

            migrationBuilder.DropTable(
                name: "Tracks");

            migrationBuilder.DropTable(
                name: "Albums");

            migrationBuilder.DropTable(
                name: "Artists");

            migrationBuilder.DropIndex(
                name: "IX_BookContributors_BookId_MediaContributorId_Role",
                table: "BookContributors");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "BookContributors",
                newName: "RoleCategory");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedOnUtc",
                table: "BookContributors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 7)
                .OldAnnotation("Relational:ColumnOrder", 6);

            migrationBuilder.AlterColumn<Guid>(
                name: "UpdatedBy",
                table: "BookContributors",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true)
                .Annotation("Relational:ColumnOrder", 8)
                .OldAnnotation("Relational:ColumnOrder", 7);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedOnUtc",
                table: "BookContributors",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 5)
                .OldAnnotation("Relational:ColumnOrder", 4);

            migrationBuilder.AlterColumn<Guid>(
                name: "CreatedBy",
                table: "BookContributors",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT")
                .Annotation("Relational:ColumnOrder", 6)
                .OldAnnotation("Relational:ColumnOrder", 5);

            migrationBuilder.AlterColumn<string>(
                name: "RoleCategory",
                table: "BookContributors",
                type: "TEXT",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 50)
                .Annotation("Relational:ColumnOrder", 4)
                .OldAnnotation("Relational:ColumnOrder", 3);

            migrationBuilder.AddColumn<string>(
                name: "RoleName",
                table: "BookContributors",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("Relational:ColumnOrder", 3);

            migrationBuilder.CreateIndex(
                name: "IX_BookContributors_BookId_MediaContributorId",
                table: "BookContributors",
                columns: new[] { "BookId", "MediaContributorId" });
        }
    }
}
