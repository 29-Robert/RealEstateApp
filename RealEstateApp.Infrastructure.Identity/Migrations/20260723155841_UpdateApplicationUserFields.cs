using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealEstateApp.Infrastructure.Identity.Migrations
{
    /// <inheritdoc />
    public partial class UpdateApplicationUserFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardId",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "Users");
        }
    }
}
