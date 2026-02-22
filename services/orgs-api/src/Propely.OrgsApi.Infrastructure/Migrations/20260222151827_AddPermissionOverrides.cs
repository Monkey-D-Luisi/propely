using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Propely.OrgsApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPermissionOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "permission_overrides",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    permission = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    granted = table.Column<bool>(type: "boolean", nullable: false),
                    granted_by = table.Column<Guid>(type: "uuid", nullable: false),
                    granted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permission_overrides", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_permission_overrides_user_org_permission",
                table: "permission_overrides",
                columns: new[] { "user_id", "organization_id", "permission" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "permission_overrides");
        }
    }
}
