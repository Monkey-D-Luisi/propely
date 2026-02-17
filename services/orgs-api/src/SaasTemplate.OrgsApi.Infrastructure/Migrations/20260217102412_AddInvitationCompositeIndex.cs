using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasTemplate.OrgsApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitationCompositeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "idx_invitations_org_email_status",
                table: "invitations",
                columns: new[] { "organization_id", "email", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_invitations_org_email_status",
                table: "invitations");
        }
    }
}
