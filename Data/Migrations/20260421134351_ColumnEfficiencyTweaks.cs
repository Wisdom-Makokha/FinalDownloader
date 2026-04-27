using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinalDownloader.Data.Migrations
{
    /// <inheritdoc />
    public partial class ColumnEfficiencyTweaks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Genres",
                table: "MediaMetadata");

            migrationBuilder.AlterColumn<int>(
                name: "Duration",
                table: "MediaMetadata",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "MediaMetadata",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "MediaMetadata");

            migrationBuilder.AlterColumn<double>(
                name: "Duration",
                table: "MediaMetadata",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Genres",
                table: "MediaMetadata",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
