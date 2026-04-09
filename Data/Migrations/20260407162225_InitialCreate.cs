using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalDownloader.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FolderPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Resolution = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Subtitles = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaContainers",
                columns: table => new
                {
                    UniqueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Album = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Artist = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Artists = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Year = table.Column<int>(type: "int", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaContainers", x => x.UniqueId);
                });

            migrationBuilder.CreateTable(
                name: "MediaMetadata",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PlaylistIndex = table.Column<int>(type: "int", nullable: true),
                    ReleaseYear = table.Column<int>(type: "int", nullable: true),
                    Artist = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Artists = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Genre = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Genres = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Uploader = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UploadDate = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    DownloadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Duration = table.Column<double>(type: "float", nullable: false),
                    MediaFormat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MediaContainerBaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaMetadata_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MediaMetadata_MediaContainers_MediaContainerBaseId",
                        column: x => x.MediaContainerBaseId,
                        principalTable: "MediaContainers",
                        principalColumn: "UniqueId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_FolderPath",
                table: "Categories",
                column: "FolderPath",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaMetadata_CategoryId",
                table: "MediaMetadata",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaMetadata_MediaContainerBaseId",
                table: "MediaMetadata",
                column: "MediaContainerBaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MediaMetadata");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "MediaContainers");
        }
    }
}
