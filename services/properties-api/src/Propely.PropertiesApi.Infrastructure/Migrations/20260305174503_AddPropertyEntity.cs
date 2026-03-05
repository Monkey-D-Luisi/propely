using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Propely.PropertiesApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    payload = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    causation_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.id);
                    table.CheckConstraint("chk_event_type", "event_type <> ''");
                });

            migrationBuilder.CreateTable(
                name: "processed_events",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    processed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_events", x => x.event_id);
                });

            migrationBuilder.CreateTable(
                name: "properties",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    property_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    operation_type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agent_id = table.Column<Guid>(type: "uuid", nullable: false),
                    agency_id = table.Column<Guid>(type: "uuid", nullable: true),
                    description_es = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    description_pt = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    description_en = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    description_fr = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    description_de = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    description_nl = table.Column<string>(type: "character varying(10000)", maxLength: 10000, nullable: true),
                    address_street = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    address_city = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    address_province = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    address_postal_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address_country = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    address_province_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    address_municipality_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    address_latitude = table.Column<double>(type: "double precision", nullable: true),
                    address_longitude = table.Column<double>(type: "double precision", nullable: true),
                    features_bedrooms = table.Column<int>(type: "integer", nullable: true),
                    features_bathrooms = table.Column<int>(type: "integer", nullable: true),
                    features_built_area = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    features_usable_area = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    features_plot_area = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    features_floor = table.Column<int>(type: "integer", nullable: true),
                    features_orientation = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    features_year_built = table.Column<int>(type: "integer", nullable: true),
                    features_energy_rating = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    features_energy_consumption = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    features_energy_emissions = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    features_has_pool = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    features_has_garden = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    features_has_garage = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    features_has_elevator = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    features_has_terrace = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    features_air_conditioning = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    features_heating = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    features_furnished = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    features_parking_spaces = table.Column<int>(type: "integer", nullable: true),
                    financials_price = table.Column<decimal>(type: "numeric(14,2)", precision: 14, scale: 2, nullable: true),
                    financials_community_fees = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    financials_ibi_tax = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: true),
                    financials_catastro_reference = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    virtual_tour_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    video_url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    published_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_properties", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_outbox_unprocessed",
                table: "outbox_messages",
                column: "occurred_at_utc",
                filter: "processed_at_utc IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_processed_events_event_type",
                table: "processed_events",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "ix_properties_agent_id",
                table: "properties",
                column: "agent_id");

            migrationBuilder.CreateIndex(
                name: "ix_properties_property_type",
                table: "properties",
                column: "property_type");

            migrationBuilder.CreateIndex(
                name: "ix_properties_status",
                table: "properties",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_properties_tenant_id",
                table: "properties",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "ix_properties_tenant_status",
                table: "properties",
                columns: new[] { "tenant_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "processed_events");

            migrationBuilder.DropTable(
                name: "properties");
        }
    }
}
