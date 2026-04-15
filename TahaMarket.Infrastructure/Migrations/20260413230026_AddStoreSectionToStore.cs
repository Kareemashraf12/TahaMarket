using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TahaMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreSectionToStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "CloseTime",
                table: "Stores",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Stores",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "OpenTime",
                table: "Stores",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreSectionId",
                table: "Stores",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "storeSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_storeSections", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stores_StoreSectionId",
                table: "Stores",
                column: "StoreSectionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Stores_storeSections_StoreSectionId",
                table: "Stores",
                column: "StoreSectionId",
                principalTable: "storeSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stores_storeSections_StoreSectionId",
                table: "Stores");

            migrationBuilder.DropTable(
                name: "storeSections");

            migrationBuilder.DropIndex(
                name: "IX_Stores_StoreSectionId",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "CloseTime",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "OpenTime",
                table: "Stores");

            migrationBuilder.DropColumn(
                name: "StoreSectionId",
                table: "Stores");
        }
    }
}
