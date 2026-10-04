using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaStock.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserProgressStatuses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserCinemaProgress_Status",
                table: "UserMediaProgress",
                newName: "WatchStatus");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "UserMediaProgress",
                newName: "PlayStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WatchStatus",
                table: "UserMediaProgress",
                newName: "UserCinemaProgress_Status");

            migrationBuilder.RenameColumn(
                name: "PlayStatus",
                table: "UserMediaProgress",
                newName: "Status");
        }
    }
}
