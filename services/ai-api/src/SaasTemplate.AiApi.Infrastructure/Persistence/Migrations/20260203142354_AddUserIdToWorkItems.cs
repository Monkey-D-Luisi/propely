using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasTemplate.AiApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToWorkItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "work_items_read",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "work_items",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "user_id",
                table: "work_items_read");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "work_items");
        }
    }
}
