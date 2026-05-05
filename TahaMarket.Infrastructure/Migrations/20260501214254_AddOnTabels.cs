using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TahaMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOnTabels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Discount",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "VariantName",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CartItemAddOns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CartItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AddOnOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItemAddOns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItemAddOns_CartItems_CartItemId",
                        column: x => x.CartItemId,
                        principalTable: "CartItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItemAddOns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItemAddOns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItemAddOns_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItemAddOns_CartItemId",
                table: "CartItemAddOns",
                column: "CartItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItemAddOns_OrderItemId",
                table: "OrderItemAddOns",
                column: "OrderItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItemAddOns");

            migrationBuilder.DropTable(
                name: "OrderItemAddOns");

            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "Discount",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "VariantName",
                table: "OrderItems");
        }
    }
}
