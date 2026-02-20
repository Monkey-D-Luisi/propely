using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Propely.OrgsApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipIndexFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_memberships_user_org",
                table: "memberships");

            migrationBuilder.CreateIndex(
                name: "idx_memberships_user_org",
                table: "memberships",
                columns: new[] { "user_id", "organization_id" },
                unique: true,
                filter: "is_deleted = FALSE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_memberships_user_org",
                table: "memberships");

            migrationBuilder.CreateIndex(
                name: "idx_memberships_user_org",
                table: "memberships",
                columns: new[] { "user_id", "organization_id" },
                unique: true);
        }
    }
}
