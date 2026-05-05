using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TahaMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCartRelationsAndOthers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId_StoreId",
                table: "Carts",
                columns: new[] { "UserId", "StoreId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Carts_UserId_StoreId",
                table: "Carts");
        }
    }
}
