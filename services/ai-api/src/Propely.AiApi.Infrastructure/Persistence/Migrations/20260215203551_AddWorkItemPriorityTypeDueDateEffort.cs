// Copyright (c) 2026 SaaS Starter Kit. All rights reserved.
// Licensed under the Proprietary Software License. See LICENSE.

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Propely.AiApi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkItemPriorityTypeDueDateEffort : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "due_date_utc",
                table: "work_items_read");

            migrationBuilder.DropColumn(
                name: "estimated_effort",
                table: "work_items_read");

            migrationBuilder.DropColumn(
                name: "priority",
                table: "work_items_read");

            migrationBuilder.DropColumn(
                name: "type",
                table: "work_items_read");

            migrationBuilder.DropColumn(
                name: "due_date_utc",
                table: "work_items");

            migrationBuilder.DropColumn(
                name: "estimated_effort",
                table: "work_items");

            migrationBuilder.DropColumn(
                name: "priority",
                table: "work_items");

            migrationBuilder.DropColumn(
                name: "type",
                table: "work_items");
        }
    }
}
