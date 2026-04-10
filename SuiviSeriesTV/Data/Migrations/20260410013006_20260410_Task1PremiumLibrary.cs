using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuiviSeriesTV.Data.Migrations
{
    /// <inheritdoc />
    public partial class _20260410_Task1PremiumLibrary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AverageEpisodeRuntimeMinutes",
                table: "Series",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackdropUrl",
                table: "Series",
                type: "TEXT",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContentType",
                table: "Series",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "Series",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastWatchedEpisode",
                table: "Series",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastWatchedSeason",
                table: "Series",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextReleaseDate",
                table: "Series",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalComment",
                table: "Series",
                type: "TEXT",
                maxLength: 1200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReleaseDate",
                table: "Series",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TmdbId",
                table: "Series",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Series_NextReleaseDate",
                table: "Series",
                column: "NextReleaseDate");

            migrationBuilder.CreateIndex(
                name: "IX_Series_OwnerId_Status_ContentType",
                table: "Series",
                columns: new[] { "OwnerId", "Status", "ContentType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Series_NextReleaseDate",
                table: "Series");

            migrationBuilder.DropIndex(
                name: "IX_Series_OwnerId_Status_ContentType",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "AverageEpisodeRuntimeMinutes",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "BackdropUrl",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "IsFavorite",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "LastWatchedEpisode",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "LastWatchedSeason",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "NextReleaseDate",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "PersonalComment",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "ReleaseDate",
                table: "Series");

            migrationBuilder.DropColumn(
                name: "TmdbId",
                table: "Series");
        }
    }
}
