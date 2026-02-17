using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SaasTemplate.OrgsApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationIdToAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "organization_id",
                table: "audit_logs",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_audit_logs_organization_id",
                table: "audit_logs",
                column: "organization_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_audit_logs_organization_id",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "organization_id",
                table: "audit_logs");
        }
    }
}
