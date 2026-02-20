using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Propely.OrgsApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at_utc",
                table: "organizations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "organizations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at_utc",
                table: "memberships",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "memberships",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at_utc",
                table: "invitations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "invitations",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted_at_utc",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "organizations");

            migrationBuilder.DropColumn(
                name: "deleted_at_utc",
                table: "memberships");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "memberships");

            migrationBuilder.DropColumn(
                name: "deleted_at_utc",
                table: "invitations");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "invitations");
        }
    }
}
