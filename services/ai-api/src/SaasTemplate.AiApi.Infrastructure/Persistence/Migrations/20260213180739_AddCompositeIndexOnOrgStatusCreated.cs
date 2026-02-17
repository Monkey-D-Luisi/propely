using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasTemplate.AiApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCompositeIndexOnOrgStatusCreated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "idx_work_items_read_org_status_created",
                table: "work_items_read",
                columns: new[] { "org_id", "status", "created_at_utc" },
                descending: new[] { false, false, true });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_work_items_read_org_status_created",
                table: "work_items_read");
        }
    }
}
