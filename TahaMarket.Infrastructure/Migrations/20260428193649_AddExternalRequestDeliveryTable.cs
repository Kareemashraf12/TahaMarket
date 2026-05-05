using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TahaMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExternalRequestDeliveryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "StockQuantity",
                table: "ProductVariants",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsStockTracked",
                table: "ProductVariants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ExternalDeliveryRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAssigned = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalDeliveryRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalDeliveryRequests_Stores_StoreId",
                        column: x => x.StoreId,
                        principalTable: "Stores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalDeliveryRequests_StoreId",
                table: "ExternalDeliveryRequests",
                column: "StoreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalDeliveryRequests");

            migrationBuilder.DropColumn(
                name: "IsStockTracked",
                table: "ProductVariants");

            migrationBuilder.AlterColumn<int>(
                name: "StockQuantity",
                table: "ProductVariants",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
