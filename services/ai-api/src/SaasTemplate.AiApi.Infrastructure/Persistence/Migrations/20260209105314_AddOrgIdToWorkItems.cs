using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasTemplate.AiApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrgIdToWorkItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Add org_id as nullable to work_items
            migrationBuilder.AddColumn<Guid>(
                name: "org_id",
                table: "work_items",
                type: "uuid",
                nullable: true);

            // Step 2: Backfill existing rows with a default org (Guid with value 1 = dev org)
            // WARNING: This backfill uses a hardcoded development GUID. Before deploying to production
            // with real multi-tenant data, a proper migration strategy must be implemented to assign
            // each existing work item to its correct organization. This is acceptable for development
            // and testing environments only.
            migrationBuilder.Sql(
                "UPDATE work_items SET org_id = '00000000-0000-0000-0000-000000000001' WHERE org_id IS NULL;");

            // Step 3: Alter column to non-nullable (no persistent default)
            migrationBuilder.AlterColumn<Guid>(
                name: "org_id",
                table: "work_items",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            // Add org_id to work_items_read (same 3-step approach)
            migrationBuilder.AddColumn<Guid>(
                name: "org_id",
                table: "work_items_read",
                type: "uuid",
                nullable: true);

            // Same dev-only backfill as work_items (see warning above)
            migrationBuilder.Sql(
                "UPDATE work_items_read SET org_id = '00000000-0000-0000-0000-000000000001' WHERE org_id IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "org_id",
                table: "work_items_read",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            // Add indexes for tenant-scoped queries
            migrationBuilder.CreateIndex(
                name: "idx_work_items_org_id",
                table: "work_items",
                column: "org_id");

            migrationBuilder.CreateIndex(
                name: "idx_work_items_read_org_id",
                table: "work_items_read",
                column: "org_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_work_items_read_org_id",
                table: "work_items_read");

            migrationBuilder.DropIndex(
                name: "idx_work_items_org_id",
                table: "work_items");

            migrationBuilder.DropColumn(
                name: "org_id",
                table: "work_items_read");

            migrationBuilder.DropColumn(
                name: "org_id",
                table: "work_items");
        }
    }
}
