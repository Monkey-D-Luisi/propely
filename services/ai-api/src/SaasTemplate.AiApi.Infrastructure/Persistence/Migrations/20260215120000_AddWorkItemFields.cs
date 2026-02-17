using Microsoft.EntityFrameworkCore.Migrations;

#nullable enable

namespace SaasTemplate.AiApi.Infrastructure.Persistence.Migrations;

public partial class AddWorkItemFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "priority",
            table: "work_items",
            type: "character varying(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "type",
            table: "work_items",
            type: "character varying(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "due_date_utc",
            table: "work_items",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "estimated_effort",
            table: "work_items",
            type: "character varying(5)",
            maxLength: 5,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "priority",
            table: "work_items_read",
            type: "character varying(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "type",
            table: "work_items_read",
            type: "character varying(20)",
            maxLength: 20,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "due_date_utc",
            table: "work_items_read",
            type: "timestamp with time zone",
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "estimated_effort",
            table: "work_items_read",
            type: "character varying(5)",
            maxLength: 5,
            nullable: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "priority", table: "work_items");
        migrationBuilder.DropColumn(name: "type", table: "work_items");
        migrationBuilder.DropColumn(name: "due_date_utc", table: "work_items");
        migrationBuilder.DropColumn(name: "estimated_effort", table: "work_items");
        migrationBuilder.DropColumn(name: "priority", table: "work_items_read");
        migrationBuilder.DropColumn(name: "type", table: "work_items_read");
        migrationBuilder.DropColumn(name: "due_date_utc", table: "work_items_read");
        migrationBuilder.DropColumn(name: "estimated_effort", table: "work_items_read");
    }
}
