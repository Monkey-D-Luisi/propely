using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasTemplate.AiApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at_utc",
                table: "work_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "work_items",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Backfill: mark existing rows with status='Deleted' as soft-deleted
            migrationBuilder.Sql(
                """
                UPDATE work_items
                SET is_deleted = TRUE,
                    deleted_at_utc = updated_at_utc
                WHERE status = 'Deleted'
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted_at_utc",
                table: "work_items");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "work_items");
        }
    }
}
