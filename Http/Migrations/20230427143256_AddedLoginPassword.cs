using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Http.Migrations
{
    /// <inheritdoc />
    public partial class AddedLoginPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Login",
                table: "NpUsers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "NpUsers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Login",
                table: "NpUsers");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "NpUsers");
        }
    }
}
