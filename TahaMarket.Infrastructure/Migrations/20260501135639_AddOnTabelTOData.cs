using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TahaMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOnTabelTOData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddOnGroups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddOnGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AddOnGroups_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AddOnGroups_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AddOnOptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AddOnGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddOnOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AddOnOptions_AddOnGroups_AddOnGroupId",
                        column: x => x.AddOnGroupId,
                        principalTable: "AddOnGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddOnGroups_ProductId",
                table: "AddOnGroups",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_AddOnGroups_StoreId",
                table: "AddOnGroups",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_AddOnOptions_AddOnGroupId",
                table: "AddOnOptions",
                column: "AddOnGroupId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddOnOptions");

            migrationBuilder.DropTable(
                name: "AddOnGroups");
        }
    }
}
