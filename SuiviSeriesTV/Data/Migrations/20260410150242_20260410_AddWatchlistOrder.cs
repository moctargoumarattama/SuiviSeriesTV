using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuiviSeriesTV.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20260410_AddWatchlistOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WatchlistOrder",
                table: "Series",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Series_OwnerId_Status_WatchlistOrder",
                table: "Series",
                columns: new[] { "OwnerId", "Status", "WatchlistOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Series_OwnerId_Status_WatchlistOrder",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "WatchlistOrder",
                table: "Series");
        }
    }
}
