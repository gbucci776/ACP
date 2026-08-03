using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ACP.Migrations
{
    /// <inheritdoc />
    public partial class AddClientOrganizations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientOrganizationId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClientOrganizations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompanyName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClientNumber = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientOrganizations", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ClientOrganizationId",
                table: "AspNetUsers",
                column: "ClientOrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientOrganizations_CompanyName",
                table: "ClientOrganizations",
                column: "CompanyName");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_ClientOrganizations_ClientOrganizationId",
                table: "AspNetUsers",
                column: "ClientOrganizationId",
                principalTable: "ClientOrganizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_ClientOrganizations_ClientOrganizationId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ClientOrganizations");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ClientOrganizationId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ClientOrganizationId",
                table: "AspNetUsers");
        }
    }
}
