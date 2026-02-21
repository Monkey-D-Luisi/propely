using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Propely.OrgsApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAgencyEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "agency_id",
                table: "organizations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "agencies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agencies", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_agencies_slug_unique",
                table: "agencies",
                column: "slug",
                unique: true,
                filter: "is_deleted = false");

            // Rename MembershipRole.Member to MembershipRole.Agent in existing data
            migrationBuilder.Sql("UPDATE memberships SET role = 'Agent' WHERE role = 'Member'");
            migrationBuilder.Sql("UPDATE invitations SET role = 'Agent' WHERE role = 'Member'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agencies");

            migrationBuilder.DropColumn(
                name: "agency_id",
                table: "organizations");

            // Revert MembershipRole.Agent back to MembershipRole.Member
            migrationBuilder.Sql("UPDATE memberships SET role = 'Member' WHERE role = 'Agent'");
            migrationBuilder.Sql("UPDATE invitations SET role = 'Member' WHERE role = 'Agent'");
        }
    }
}
