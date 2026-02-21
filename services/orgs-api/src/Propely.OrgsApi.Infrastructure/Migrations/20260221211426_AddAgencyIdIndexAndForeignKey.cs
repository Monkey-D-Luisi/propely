using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Propely.OrgsApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAgencyIdIndexAndForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_organizations_agency_id",
                table: "organizations",
                column: "agency_id");

            migrationBuilder.AddForeignKey(
                name: "fk_organizations_agencies_agency_id",
                table: "organizations",
                column: "agency_id",
                principalTable: "agencies",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_organizations_agencies_agency_id",
                table: "organizations");

            migrationBuilder.DropIndex(
                name: "ix_organizations_agency_id",
                table: "organizations");
        }
    }
}
