using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasTemplate.OrgsApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordVersionToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "password_version",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "password_version",
                table: "users");
        }
    }
}
